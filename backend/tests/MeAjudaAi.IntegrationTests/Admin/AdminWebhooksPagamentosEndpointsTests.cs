using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.DTOs.Impulsionamentos;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.IntegrationTests.Admin;

public class AdminWebhooksPagamentosEndpointsTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    private const string SegredoWebhook = "segredo-webhook-teste";
    private const string HeaderAssinatura = "X-Webhook-Signature";

    private readonly TestWebApplicationFactory _factory;

    public AdminWebhooksPagamentosEndpointsTests(TestWebApplicationFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Buscar_DeveAplicarFiltrosEPaginacao()
    {
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();
        using var webhookClient = _factory.CreateClient();

        var profissional = await RegistrarProfissionalAsync(profissionalClient, "prof-admin-webhooks-lista");
        var admin = await LoginAdminAsync(adminClient);

        profissionalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", profissional.Auth.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var planoId = await ObterPlanoIdAsync();
        var contratarResponse = await profissionalClient.PostAsJsonAsync("/api/impulsionamentos/contratar", new
        {
            planoImpulsionamentoId = planoId,
            codigoReferenciaPagamento = "admin-webhook-lista"
        });

        Assert.Equal(HttpStatusCode.OK, contratarResponse.StatusCode);
        var impulsionamento = await contratarResponse.Content.ReadFromJsonAsync<ImpulsionamentoProfissionalResponse>();
        Assert.NotNull(impulsionamento);

        var webhookRequest = CriarWebhookRequest("admin-webhook-lista", "admin-webhook-lista-evt");
        var webhookResponse = await webhookClient.SendAsync(webhookRequest);
        Assert.Equal(HttpStatusCode.OK, webhookResponse.StatusCode);

        var response = await adminClient.GetAsync(
            $"/api/admin/webhooks/pagamentos?eventoExternoId=admin-webhook-lista-evt&codigoReferenciaPagamento=admin-webhook-lista&provedor=padrao&processadoComSucesso=true&impulsionamentoProfissionalId={impulsionamento!.Id}&pagina=1&tamanhoPagina=10");
        var payload = await response.Content.ReadFromJsonAsync<PaginacaoResponse<WebhookPagamentoImpulsionamentoEventoResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.True(payload!.TotalRegistros >= 1);
        Assert.Contains(payload.Itens, x =>
            x.EventoExternoId == "admin-webhook-lista-evt" &&
            x.CodigoReferenciaPagamento == "admin-webhook-lista" &&
            x.ImpulsionamentoProfissionalId == impulsionamento.Id);
    }

    [Fact]
    public async Task ObterPorId_DeveRetornarDetalhe()
    {
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();
        using var webhookClient = _factory.CreateClient();

        var profissional = await RegistrarProfissionalAsync(profissionalClient, "prof-admin-webhooks-detalhe");
        var admin = await LoginAdminAsync(adminClient);

        profissionalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", profissional.Auth.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var planoId = await ObterPlanoIdAsync();
        var contratarResponse = await profissionalClient.PostAsJsonAsync("/api/impulsionamentos/contratar", new
        {
            planoImpulsionamentoId = planoId,
            codigoReferenciaPagamento = "admin-webhook-detalhe"
        });

        Assert.Equal(HttpStatusCode.OK, contratarResponse.StatusCode);

        var webhookRequest = CriarWebhookRequest("admin-webhook-detalhe", "admin-webhook-detalhe-evt");
        var webhookResponse = await webhookClient.SendAsync(webhookRequest);
        Assert.Equal(HttpStatusCode.OK, webhookResponse.StatusCode);

        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MeAjudaAi.Infrastructure.Persistence.Contexts.AppDbContext>();
        var webhookId = await context.WebhookPagamentoImpulsionamentoEventos
            .Where(x => x.EventoExternoId == "admin-webhook-detalhe-evt")
            .Select(x => x.Id)
            .SingleAsync();

        var response = await adminClient.GetAsync($"/api/admin/webhooks/pagamentos/{webhookId}");
        var payload = await response.Content.ReadFromJsonAsync<WebhookPagamentoAdminDetalheResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(webhookId, payload!.Id);
        Assert.Equal("admin-webhook-detalhe-evt", payload.EventoExternoId);
        Assert.Equal("admin-webhook-detalhe", payload.CodigoReferenciaPagamento);
        Assert.Contains("admin-webhook-detalhe-evt", payload.PayloadJson);
        Assert.Contains(HeaderAssinatura, payload.HeadersJson);
    }

    [Fact]
    public async Task ObterPorId_Inexistente_DeveRetornar404()
    {
        using var adminClient = _factory.CreateClient();
        var admin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var response = await adminClient.GetAsync($"/api/admin/webhooks/pagamentos/{Guid.NewGuid()}");
        var payload = await response.Content.ReadFromJsonAsync<MeAjudaAi.Application.DTOs.Common.MensagemErroResponse>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("Webhook de pagamento não encontrado.", payload!.Mensagem);
    }

    private static HttpRequestMessage CriarWebhookRequest(string codigoReferenciaPagamento, string eventoExternoId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/webhooks/pagamentos/impulsionamentos")
        {
            Content = JsonContent.Create(new WebhookPagamentoImpulsionamentoRequest
            {
                CodigoReferenciaPagamento = codigoReferenciaPagamento,
                StatusPagamento = "pago",
                EventoExternoId = eventoExternoId
            })
        };
        request.Headers.UserAgent.ParseAdd("integration-test-agent/1.0");

        var payload = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SegredoWebhook));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        request.Headers.Add(HeaderAssinatura, Convert.ToHexString(hash).ToLowerInvariant());

        return request;
    }

    private async Task<Guid> ObterPlanoIdAsync()
    {
        using var client = _factory.CreateClient();
        var planos = await client.GetFromJsonAsync<List<PlanoImpulsionamentoResponse>>("/api/impulsionamentos/planos");
        return planos![0].Id;
    }

    private static async Task<AuthResponse> LoginAdminAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = TestWebApplicationFactory.EmailAdmin,
            senha = TestWebApplicationFactory.SenhaAdmin
        });

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    private async Task<ProfissionalRegistrado> RegistrarProfissionalAsync(HttpClient client, string prefixo)
    {
        var email = $"{prefixo}-{Guid.NewGuid():N}@teste.local";
        const string senha = "Senha@123";

        var response = await client.PostAsJsonAsync("/api/auth/registrar", new RegistrarUsuarioRequest
        {
            Nome = $"{prefixo} teste",
            Email = email,
            Telefone = "11999999999",
            Senha = senha,
            TipoPerfil = TipoPerfil.Profissional
        });

        response.EnsureSuccessStatusCode();

        var auth = (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
        var profissionalId = await _factory.ObterProfissionalIdPorUsuarioIdAsync(auth.UsuarioId);
        return new ProfissionalRegistrado(auth, profissionalId);
    }

    private sealed record ProfissionalRegistrado(AuthResponse Auth, Guid ProfissionalId);
}
