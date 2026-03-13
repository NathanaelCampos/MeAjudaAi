using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Application.DTOs.Common;
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
}
