using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.DTOs.Impulsionamentos;
using MeAjudaAi.Application.DTOs.Notificacoes;
using MeAjudaAi.Application.DTOs.Servicos;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.IntegrationTests.Infrastructure;

namespace MeAjudaAi.IntegrationTests.Admin;

public class AdminProfissionaisEndpointsTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AdminProfissionaisEndpointsTests(TestWebApplicationFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task BuscarProfissionais_DeveAplicarFiltrosEPaginacao()
    {
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        await RegistrarProfissionalAsync(profissionalClient, "prof-admin-prof-lista");
        var admin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var response = await adminClient.GetAsync("/api/admin/profissionais?nome=prof-admin-prof-lista&pagina=1&tamanhoPagina=10");
        var payload = await response.Content.ReadFromJsonAsync<PaginacaoResponse<ProfissionalAdminListItemResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.True(payload!.TotalRegistros >= 1);
        Assert.NotEmpty(payload.Itens);
    }

    [Fact]
    public async Task ObterProfissionalPorId_DeveRetornarDetalhe()
    {
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var profissional = await RegistrarProfissionalAsync(profissionalClient, "prof-admin-prof-detalhe");
        var admin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var response = await adminClient.GetAsync($"/api/admin/profissionais/{profissional.ProfissionalId}");
        var payload = await response.Content.ReadFromJsonAsync<ProfissionalAdminDetalheResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(profissional.ProfissionalId, payload!.Id);
        Assert.Equal(profissional.Auth.UsuarioId, payload.UsuarioId);
    }

    [Fact]
    public async Task VerificarEDesverificarProfissional_DeveAtualizarFlag()
    {
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var profissional = await RegistrarProfissionalAsync(profissionalClient, "prof-admin-prof-verificar");
        var admin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var verificar = await adminClient.PutAsync($"/api/admin/profissionais/{profissional.ProfissionalId}/verificar", null);
        var verificado = await verificar.Content.ReadFromJsonAsync<ProfissionalAdminDetalheResponse>();

        Assert.Equal(HttpStatusCode.OK, verificar.StatusCode);
        Assert.NotNull(verificado);
        Assert.True(verificado!.PerfilVerificado);

        var desverificar = await adminClient.PutAsync($"/api/admin/profissionais/{profissional.ProfissionalId}/desverificar", null);
        var desverificado = await desverificar.Content.ReadFromJsonAsync<ProfissionalAdminDetalheResponse>();

        Assert.Equal(HttpStatusCode.OK, desverificar.StatusCode);
        Assert.NotNull(desverificado);
        Assert.False(desverificado!.PerfilVerificado);
    }

    [Fact]
    public async Task DesativarProfissional_DeveInativarUsuarioEImpedirLogin()
    {
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var profissional = await RegistrarProfissionalAsync(profissionalClient, "prof-admin-prof-desativar");
        var admin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var desativar = await adminClient.PutAsync($"/api/admin/profissionais/{profissional.ProfissionalId}/desativar", null);
        var inativo = await desativar.Content.ReadFromJsonAsync<ProfissionalAdminDetalheResponse>();

        Assert.Equal(HttpStatusCode.OK, desativar.StatusCode);
        Assert.NotNull(inativo);
        Assert.False(inativo!.Ativo);

        var login = await profissionalClient.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = profissional.Email,
            Senha = profissional.Senha
        });

        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    [Fact]
    public async Task AtivarProfissional_DevePermitirNovoLogin()
    {
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var profissional = await RegistrarProfissionalAsync(profissionalClient, "prof-admin-prof-ativar");
        var admin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var desativar = await adminClient.PutAsync($"/api/admin/profissionais/{profissional.ProfissionalId}/desativar", null);
        Assert.Equal(HttpStatusCode.OK, desativar.StatusCode);

        var ativar = await adminClient.PutAsync($"/api/admin/profissionais/{profissional.ProfissionalId}/ativar", null);
        var ativo = await ativar.Content.ReadFromJsonAsync<ProfissionalAdminDetalheResponse>();

        Assert.Equal(HttpStatusCode.OK, ativar.StatusCode);
        Assert.NotNull(ativo);
        Assert.True(ativo!.Ativo);

        var login = await profissionalClient.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = profissional.Email,
            Senha = profissional.Senha
        });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task ObterDashboardProfissional_DeveConsolidarIndicadores()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var cliente = await RegistrarClienteAsync(clienteClient, "cliente-admin-prof-dashboard");
        var profissional = await RegistrarProfissionalAsync(profissionalClient, "prof-admin-prof-dashboard");
        var admin = await LoginAdminAsync(adminClient);

        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cliente.Auth.Token);
        profissionalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", profissional.Auth.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var preferenciasResponse = await profissionalClient.PutAsJsonAsync("/api/notificacoes/minhas/preferencias", new AtualizarPreferenciasNotificacaoRequest
        {
            Preferencias = new[]
            {
                new PreferenciaNotificacaoItemRequest
                {
                    Tipo = TipoNotificacao.ServicoSolicitado,
                    AtivoInterno = true,
                    AtivoEmail = true
                }
            }
        });

        Assert.Equal(HttpStatusCode.OK, preferenciasResponse.StatusCode);

        var cidadeId = await _factory.ObterCidadeIdAsync();

        var criarServicoResponse = await clienteClient.PostAsJsonAsync("/api/servicos", new CriarServicoRequest
        {
            ProfissionalId = profissional.ProfissionalId,
            CidadeId = cidadeId,
            Titulo = "Servico para dashboard admin profissional",
            Descricao = "Gera dados para dashboard admin profissional",
            ValorCombinado = 200m
        });

        Assert.Equal(HttpStatusCode.OK, criarServicoResponse.StatusCode);

        var planos = await profissionalClient.GetFromJsonAsync<IReadOnlyList<PlanoImpulsionamentoResponse>>("/api/impulsionamentos/planos");
        Assert.NotNull(planos);
        Assert.NotEmpty(planos!);

        var contratarResponse = await profissionalClient.PostAsJsonAsync("/api/impulsionamentos/contratar", new ContratarPlanoImpulsionamentoRequest
        {
            PlanoImpulsionamentoId = planos![0].Id,
            CodigoReferenciaPagamento = $"admin-prof-dashboard-{Guid.NewGuid():N}"
        });

        Assert.Equal(HttpStatusCode.OK, contratarResponse.StatusCode);

        var dashboardResponse = await adminClient.GetAsync($"/api/admin/profissionais/{profissional.ProfissionalId}/dashboard");
        var dashboard = await dashboardResponse.Content.ReadFromJsonAsync<ProfissionalAdminDashboardResponse>();

        Assert.Equal(HttpStatusCode.OK, dashboardResponse.StatusCode);
        Assert.NotNull(dashboard);
        Assert.Equal(profissional.ProfissionalId, dashboard!.Profissional.Id);
        Assert.True(dashboard.Notificacoes.TotalAtivas >= 1);
        Assert.True(dashboard.Emails.Total >= 1);
        Assert.True(dashboard.Servicos.Total >= 1);
        Assert.True(dashboard.Servicos.Solicitados >= 1);
        Assert.True(dashboard.Impulsionamentos.Total >= 1);
        Assert.True(dashboard.Impulsionamentos.PendentesPagamento >= 1);
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

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);

        var profissionalId = await _factory.ObterProfissionalIdPorUsuarioIdAsync(auth!.UsuarioId);
        return new ProfissionalRegistrado(auth, profissionalId, email, senha);
    }

    private static async Task<ClienteRegistrado> RegistrarClienteAsync(HttpClient client, string prefixo)
    {
        var email = $"{prefixo}-{Guid.NewGuid():N}@teste.local";
        const string senha = "Senha@123";

        var response = await client.PostAsJsonAsync("/api/auth/registrar", new RegistrarUsuarioRequest
        {
            Nome = $"{prefixo} teste",
            Email = email,
            Telefone = "11988887777",
            Senha = senha,
            TipoPerfil = TipoPerfil.Cliente
        });

        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);

        return new ClienteRegistrado(auth!, email, senha);
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

    private sealed record ProfissionalRegistrado(AuthResponse Auth, Guid ProfissionalId, string Email, string Senha);
    private sealed record ClienteRegistrado(AuthResponse Auth, string Email, string Senha);
}
