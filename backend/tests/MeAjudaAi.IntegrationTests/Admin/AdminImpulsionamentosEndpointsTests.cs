using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.IntegrationTests.Infrastructure;

namespace MeAjudaAi.IntegrationTests.Admin;

public class AdminImpulsionamentosEndpointsTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AdminImpulsionamentosEndpointsTests(TestWebApplicationFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task BuscarImpulsionamentos_DeveAplicarFiltrosEPaginacao()
    {
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var profissional = await RegistrarProfissionalAsync(profissionalClient, "prof-admin-impulsionamentos-lista");
        var admin = await LoginAdminAsync(adminClient);

        profissionalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", profissional.Auth.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var planoId = await ObterPlanoIdAsync();
        var contratarResponse = await profissionalClient.PostAsJsonAsync("/api/impulsionamentos/contratar", new
        {
            planoImpulsionamentoId = planoId,
            codigoReferenciaPagamento = "admin-impulsionamento-lista"
        });

        Assert.Equal(HttpStatusCode.OK, contratarResponse.StatusCode);
        var impulsionamento = await contratarResponse.Content.ReadFromJsonAsync<MeAjudaAi.Application.DTOs.Impulsionamentos.ImpulsionamentoProfissionalResponse>();
        Assert.NotNull(impulsionamento);

        var response = await adminClient.GetAsync($"/api/admin/impulsionamentos?termo=admin-impulsionamento-lista&profissionalId={profissional.ProfissionalId}&status={StatusImpulsionamento.PendentePagamento}&pagina=1&tamanhoPagina=10");
        var payload = await response.Content.ReadFromJsonAsync<PaginacaoResponse<ImpulsionamentoAdminListItemResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.True(payload!.TotalRegistros >= 1);
        Assert.Contains(payload.Itens, x => x.Id == impulsionamento!.Id && x.ProfissionalId == profissional.ProfissionalId);
    }

    [Fact]
    public async Task ObterImpulsionamentoPorId_DeveRetornarDetalhe()
    {
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var profissional = await RegistrarProfissionalAsync(profissionalClient, "prof-admin-impulsionamentos-detalhe");
        var admin = await LoginAdminAsync(adminClient);

        profissionalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", profissional.Auth.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var planoId = await ObterPlanoIdAsync();
        var contratarResponse = await profissionalClient.PostAsJsonAsync("/api/impulsionamentos/contratar", new
        {
            planoImpulsionamentoId = planoId,
            codigoReferenciaPagamento = "admin-impulsionamento-detalhe"
        });

        Assert.Equal(HttpStatusCode.OK, contratarResponse.StatusCode);
        var impulsionamento = await contratarResponse.Content.ReadFromJsonAsync<MeAjudaAi.Application.DTOs.Impulsionamentos.ImpulsionamentoProfissionalResponse>();
        Assert.NotNull(impulsionamento);

        var response = await adminClient.GetAsync($"/api/admin/impulsionamentos/{impulsionamento!.Id}");
        var payload = await response.Content.ReadFromJsonAsync<ImpulsionamentoAdminDetalheResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(impulsionamento.Id, payload!.Id);
        Assert.Equal(profissional.ProfissionalId, payload.ProfissionalId);
        Assert.Equal("admin-impulsionamento-detalhe", payload.CodigoReferenciaPagamento);
    }

    [Fact]
    public async Task ObterImpulsionamentoPorId_Inexistente_DeveRetornar404()
    {
        using var adminClient = _factory.CreateClient();
        var admin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var response = await adminClient.GetAsync($"/api/admin/impulsionamentos/{Guid.NewGuid()}");
        var payload = await response.Content.ReadFromJsonAsync<MeAjudaAi.Application.DTOs.Common.MensagemErroResponse>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("Impulsionamento não encontrado.", payload!.Mensagem);
    }

    private async Task<Guid> ObterPlanoIdAsync()
    {
        using var client = _factory.CreateClient();
        var planos = await client.GetFromJsonAsync<List<MeAjudaAi.Application.DTOs.Impulsionamentos.PlanoImpulsionamentoResponse>>("/api/impulsionamentos/planos");
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
