using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Application.DTOs.Impulsionamentos;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using MeAjudaAi.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.IntegrationTests.Impulsionamentos;

public class ImpulsionamentosWebhookTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    private const string SegredoWebhook = "meajudaai-webhook-secret-dev";
    private const string SegredoWebhookAsaas = "meajudaai-webhook-secret-dev";
    private const string HeaderAssinatura = "X-Webhook-Signature";
    private const string HeaderAssinaturaAsaas = "X-Asaas-Signature";

    private readonly TestWebApplicationFactory _factory;

    public ImpulsionamentosWebhookTests(TestWebApplicationFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Webhook_DeveRetornarUnauthorizedQuandoSegredoForInvalido()
    {
        using var client = _factory.CreateClient();
        client.ApplyAnonymous();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/webhooks/pagamentos/impulsionamentos")
        {
            Content = JsonContent.Create(new WebhookPagamentoImpulsionamentoRequest
            {
                CodigoReferenciaPagamento = "pag-webhook-001",
                StatusPagamento = "pago"
            })
        };

        request.Headers.Add(HeaderAssinatura, "assinatura-invalida");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Webhook_ComStatusPago_DeveAtivarImpulsionamento()
    {
        using var client = _factory.CreateClient();

        var auth = await RegistrarProfissionalAsync(client, "webhook-pago");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Token);

        var planos = await client.GetFromJsonAsync<List<PlanoImpulsionamentoResponse>>("/api/impulsionamentos/planos");
        Assert.NotNull(planos);

        var contratarResponse = await client.PostAsJsonAsync("/api/impulsionamentos/contratar", new ContratarPlanoImpulsionamentoRequest
        {
            PlanoImpulsionamentoId = planos![0].Id,
            CodigoReferenciaPagamento = "pag-webhook-002"
        });

        var contratado = await contratarResponse.Content.ReadFromJsonAsync<ImpulsionamentoProfissionalResponse>();
        Assert.NotNull(contratado);
        Assert.Equal(StatusImpulsionamento.PendentePagamento, contratado!.Status);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/webhooks/pagamentos/impulsionamentos")
        {
            Content = JsonContent.Create(new WebhookPagamentoImpulsionamentoRequest
            {
                CodigoReferenciaPagamento = "pag-webhook-002",
                StatusPagamento = "pago",
                EventoExternoId = "evt-pago-001"
            })
        };

        AssinarRequest(request);

        var response = await client.SendAsync(request);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Webhook processado.", json.RootElement.GetProperty("mensagem").GetString());
        Assert.Equal("padrao", json.RootElement.GetProperty("provedor").GetString());
        Assert.Equal("pago", json.RootElement.GetProperty("statusRecebido").GetString());
        Assert.Equal("evt-pago-001", json.RootElement.GetProperty("eventoExternoId").GetString());
        Assert.False(json.RootElement.GetProperty("duplicado").GetBoolean());
        Assert.Equal((int)StatusImpulsionamento.Ativo, json.RootElement.GetProperty("impulsionamento").GetProperty("status").GetInt32());

        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var evento = await context.WebhookPagamentoImpulsionamentoEventos
            .SingleAsync(x => x.EventoExternoId == "evt-pago-001");

        Assert.True(evento.ProcessadoComSucesso);
        Assert.Equal("padrao", evento.Provedor);
        Assert.Equal("pag-webhook-002", evento.CodigoReferenciaPagamento);
    }

    [Fact]
    public async Task Webhook_ComStatusCancelado_DeveCancelarImpulsionamento()
    {
        using var client = _factory.CreateClient();

        var auth = await RegistrarProfissionalAsync(client, "webhook-cancelado");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Token);

        var planos = await client.GetFromJsonAsync<List<PlanoImpulsionamentoResponse>>("/api/impulsionamentos/planos");
        Assert.NotNull(planos);

        var contratarResponse = await client.PostAsJsonAsync("/api/impulsionamentos/contratar", new ContratarPlanoImpulsionamentoRequest
        {
            PlanoImpulsionamentoId = planos![0].Id,
            CodigoReferenciaPagamento = "pag-webhook-003"
        });

        var contratado = await contratarResponse.Content.ReadFromJsonAsync<ImpulsionamentoProfissionalResponse>();
        Assert.NotNull(contratado);
        Assert.Equal(StatusImpulsionamento.PendentePagamento, contratado!.Status);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/webhooks/pagamentos/impulsionamentos")
        {
            Content = JsonContent.Create(new WebhookPagamentoImpulsionamentoRequest
            {
                CodigoReferenciaPagamento = "pag-webhook-003",
                StatusPagamento = "cancelado",
                EventoExternoId = "evt-cancelado-001"
            })
        };

        AssinarRequest(request);

        var response = await client.SendAsync(request);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("cancelado", json.RootElement.GetProperty("statusRecebido").GetString());
        Assert.Equal((int)StatusImpulsionamento.Cancelado, json.RootElement.GetProperty("impulsionamento").GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task Webhook_ComMesmoEventoExternoId_DeveRetornarRespostaIdempotente()
    {
        using var client = _factory.CreateClient();

        var auth = await RegistrarProfissionalAsync(client, "webhook-duplicado");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Token);

        var planos = await client.GetFromJsonAsync<List<PlanoImpulsionamentoResponse>>("/api/impulsionamentos/planos");
        Assert.NotNull(planos);

        await client.PostAsJsonAsync("/api/impulsionamentos/contratar", new ContratarPlanoImpulsionamentoRequest
        {
            PlanoImpulsionamentoId = planos![0].Id,
            CodigoReferenciaPagamento = "pag-webhook-004"
        });

        var primeiraRequest = new HttpRequestMessage(HttpMethod.Post, "/api/webhooks/pagamentos/impulsionamentos")
        {
            Content = JsonContent.Create(new WebhookPagamentoImpulsionamentoRequest
            {
                CodigoReferenciaPagamento = "pag-webhook-004",
                StatusPagamento = "pago",
                EventoExternoId = "evt-duplicado-001"
            })
        };

        AssinarRequest(primeiraRequest);

        var primeiraResponse = await client.SendAsync(primeiraRequest);
        Assert.Equal(HttpStatusCode.OK, primeiraResponse.StatusCode);

        var segundaRequest = new HttpRequestMessage(HttpMethod.Post, "/api/webhooks/pagamentos/impulsionamentos")
        {
            Content = JsonContent.Create(new WebhookPagamentoImpulsionamentoRequest
            {
                CodigoReferenciaPagamento = "pag-webhook-004",
                StatusPagamento = "pago",
                EventoExternoId = "evt-duplicado-001"
            })
        };

        AssinarRequest(segundaRequest);

        var segundaResponse = await client.SendAsync(segundaRequest);
        var json = JsonDocument.Parse(await segundaResponse.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, segundaResponse.StatusCode);
        Assert.Equal("Webhook já processado.", json.RootElement.GetProperty("mensagem").GetString());
        Assert.Equal("padrao", json.RootElement.GetProperty("provedor").GetString());
        Assert.True(json.RootElement.GetProperty("duplicado").GetBoolean());

        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Equal(1, await context.WebhookPagamentoImpulsionamentoEventos
            .CountAsync(x => x.EventoExternoId == "evt-duplicado-001"));
    }

    [Fact]
    public async Task Webhook_DeveAceitarConfiguracaoDeProvedorAlternativo()
    {
        using var client = _factory.CreateClient();

        var auth = await RegistrarProfissionalAsync(client, "webhook-asaas");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Token);

        var planos = await client.GetFromJsonAsync<List<PlanoImpulsionamentoResponse>>("/api/impulsionamentos/planos");
        Assert.NotNull(planos);

        await client.PostAsJsonAsync("/api/impulsionamentos/contratar", new ContratarPlanoImpulsionamentoRequest
        {
            PlanoImpulsionamentoId = planos![0].Id,
            CodigoReferenciaPagamento = "pag-webhook-asaas-001"
        });

        const string payload = """
            {
              "id": "evt-asaas-001",
              "event": "PAYMENT_RECEIVED",
              "payment": {
                "externalReference": "pag-webhook-asaas-001",
                "status": "RECEIVED"
              }
            }
            """;

        var request = CriarRequestComPayload(
            "/api/webhooks/pagamentos/asaas/impulsionamentos",
            payload,
            SegredoWebhookAsaas,
            HeaderAssinaturaAsaas);

        var response = await client.SendAsync(request);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("asaas", json.RootElement.GetProperty("provedor").GetString());
        Assert.Equal("evt-asaas-001", json.RootElement.GetProperty("eventoExternoId").GetString());

        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var evento = await context.WebhookPagamentoImpulsionamentoEventos
            .SingleAsync(x => x.EventoExternoId == "evt-asaas-001");

        Assert.Equal("asaas", evento.Provedor);
        Assert.True(evento.ProcessadoComSucesso);
        Assert.Contains("X-Asaas-Signature", evento.HeadersJson);
        Assert.NotNull(evento.IpOrigem);
        Assert.NotEmpty(evento.RequestId);
        Assert.Contains("integration-test-agent", evento.UserAgent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Webhook_Asaas_ComPayloadInvalidoDeveRetornarBadRequest()
    {
        using var client = _factory.CreateClient();

        const string payload = """
            {
              "id": "evt-asaas-invalido-001",
              "event": "PAYMENT_RECEIVED",
              "payment": {
                "status": "RECEIVED"
              }
            }
            """;

        var request = CriarRequestComPayload(
            "/api/webhooks/pagamentos/asaas/impulsionamentos",
            payload,
            SegredoWebhookAsaas,
            HeaderAssinaturaAsaas);

        var response = await client.SendAsync(request);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Payload inválido.", json.RootElement.GetProperty("mensagem").GetString());
    }

    private static async Task<AuthResponse> RegistrarProfissionalAsync(HttpClient client, string prefixo)
    {
        var response = await client.PostAsJsonAsync("/api/auth/registrar", new RegistrarUsuarioRequest
        {
            Nome = $"{prefixo} profissional",
            Email = $"{prefixo}-{Guid.NewGuid():N}@teste.local",
            Telefone = "11999999999",
            Senha = "Senha@123",
            TipoPerfil = TipoPerfil.Profissional
        });

        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);

        return auth!;
    }

    private static void AssinarRequest(
        HttpRequestMessage request,
        string segredo = SegredoWebhook,
        string headerAssinatura = HeaderAssinatura)
    {
        var payload = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(segredo));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        request.Headers.Add(headerAssinatura, Convert.ToHexString(hash).ToLowerInvariant());
    }

    private static HttpRequestMessage CriarRequestComPayload(
        string url,
        string payload,
        string segredo,
        string headerAssinatura)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        request.Headers.UserAgent.ParseAdd("integration-test-agent/1.0");
        AssinarRequest(request, segredo, headerAssinatura);

        return request;
    }
}
