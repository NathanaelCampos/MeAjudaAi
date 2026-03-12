using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.DTOs.Impulsionamentos;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.IntegrationTests.Infrastructure;

namespace MeAjudaAi.IntegrationTests.Impulsionamentos;

public class ImpulsionamentosWebhooksConsultaTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    private const string SegredoWebhookAsaas = "segredo-webhook-asaas-teste";

    private readonly TestWebApplicationFactory _factory;

    public ImpulsionamentosWebhooksConsultaTests(TestWebApplicationFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ListarWebhooks_DevePermitirFiltrarPorProvedorEEventoExternoQuandoAdministrador()
    {
        using var client = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var auth = await RegistrarProfissionalAsync(client, "consulta-webhook");
        var admin = await LoginAdminAsync(adminClient);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Token);

        adminClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", admin.Token);

        var planos = await client.GetFromJsonAsync<List<PlanoImpulsionamentoResponse>>("/api/impulsionamentos/planos");
        Assert.NotNull(planos);

        await client.PostAsJsonAsync("/api/impulsionamentos/contratar", new ContratarPlanoImpulsionamentoRequest
        {
            PlanoImpulsionamentoId = planos![0].Id,
            CodigoReferenciaPagamento = "pag-webhook-consulta-001"
        });

        const string payload = """
            {
              "id": "evt-consulta-001",
              "event": "PAYMENT_RECEIVED",
              "payment": {
                "externalReference": "pag-webhook-consulta-001",
                "status": "RECEIVED"
              }
            }
            """;

        var request = CriarRequestComPayload(
            "/api/webhooks/pagamentos/asaas/impulsionamentos",
            payload,
            SegredoWebhookAsaas);

        var webhookResponse = await client.SendAsync(request);
        webhookResponse.EnsureSuccessStatusCode();

        var response = await adminClient.GetFromJsonAsync<PaginacaoResponse<WebhookPagamentoImpulsionamentoEventoResponse>>(
            "/api/impulsionamentos/webhooks?provedor=asaas&eventoExternoId=evt-consulta-001");

        Assert.NotNull(response);
        Assert.Equal(1, response!.TotalRegistros);
        Assert.Single(response.Itens);
        Assert.Equal("asaas", response.Itens[0].Provedor);
        Assert.Equal("evt-consulta-001", response.Itens[0].EventoExternoId);
        Assert.Equal("pag-webhook-consulta-001", response.Itens[0].CodigoReferenciaPagamento);
    }

    [Fact]
    public async Task ListarWebhooks_DeveRetornarForbiddenQuandoUsuarioNaoForAdministrador()
    {
        using var client = _factory.CreateClient();

        var auth = await RegistrarProfissionalAsync(client, "consulta-webhook-auth");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Token);

        var response = await client.GetAsync("/api/impulsionamentos/webhooks");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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

    private static async Task<AuthResponse> LoginAdminAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = TestWebApplicationFactory.EmailAdmin,
            Senha = TestWebApplicationFactory.SenhaAdmin
        });

        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);

        return auth!;
    }

    private static HttpRequestMessage CriarRequestComPayload(
        string url,
        string payload,
        string segredo)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        request.Headers.UserAgent.ParseAdd("integration-test-agent/1.0");

        using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(segredo));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        request.Headers.Add("X-Asaas-Signature", Convert.ToHexString(hash).ToLowerInvariant());

        return request;
    }
}
