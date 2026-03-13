using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.DTOs.Notificacoes;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.IntegrationTests.Infrastructure;

namespace MeAjudaAi.IntegrationTests.Admin;

public class AdminUsuariosEndpointsTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AdminUsuariosEndpointsTests(TestWebApplicationFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task BuscarUsuarios_DeveAplicarFiltrosEPaginacao()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-admin-lista");
        var profissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "prof-admin-lista");
        var admin = await LoginAdminAsync(adminClient);

        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var response = await adminClient.GetAsync($"/api/admin/usuarios?tipoPerfil={TipoPerfil.Profissional}&nome=prof-admin-lista&pagina=1&tamanhoPagina=10");
        var payload = await response.Content.ReadFromJsonAsync<PaginacaoResponse<UsuarioAdminListItemResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.True(payload!.TotalRegistros >= 1);
        Assert.Contains(payload.Itens, x => x.Id == profissional.Auth.UsuarioId && x.TipoPerfil == TipoPerfil.Profissional);
    }

    [Fact]
    public async Task ObterUsuarioPorId_DeveRetornarDetalhe()
    {
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var profissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "prof-admin-detalhe");
        var admin = await LoginAdminAsync(adminClient);

        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var response = await adminClient.GetAsync($"/api/admin/usuarios/{profissional.Auth.UsuarioId}");
        var payload = await response.Content.ReadFromJsonAsync<UsuarioAdminDetalheResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(profissional.Auth.UsuarioId, payload!.Id);
        Assert.Equal(TipoPerfil.Profissional, payload.TipoPerfil);
        Assert.NotNull(payload.ProfissionalId);
    }

    [Fact]
    public async Task BloquearUsuario_DeveImpedirLoginENegarAcessoComTokenAtual()
    {
        using var clienteClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var cliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-admin-bloqueio");
        var admin = await LoginAdminAsync(adminClient);

        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cliente.Auth.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var antes = await clienteClient.GetAsync("/api/notificacoes/minhas");
        Assert.Equal(HttpStatusCode.OK, antes.StatusCode);

        var bloquear = await adminClient.PutAsync($"/api/admin/usuarios/{cliente.Auth.UsuarioId}/bloquear", null);
        var bloqueado = await bloquear.Content.ReadFromJsonAsync<UsuarioAdminDetalheResponse>();

        Assert.Equal(HttpStatusCode.OK, bloquear.StatusCode);
        Assert.NotNull(bloqueado);
        Assert.False(bloqueado!.Ativo);

        var loginBloqueado = await clienteClient.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = cliente.Email,
            Senha = cliente.Senha
        });

        var acessoComTokenAtual = await clienteClient.GetAsync("/api/notificacoes/minhas");
        var erroAcesso = await acessoComTokenAtual.Content.ReadFromJsonAsync<MeAjudaAi.Application.DTOs.Common.MensagemErroResponse>();

        Assert.Equal(HttpStatusCode.Unauthorized, loginBloqueado.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, acessoComTokenAtual.StatusCode);
        Assert.NotNull(erroAcesso);
        Assert.Equal("Usuário inativo.", erroAcesso!.Mensagem);
    }

    [Fact]
    public async Task DesbloquearUsuario_DevePermitirNovoLogin()
    {
        using var clienteClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var cliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-admin-desbloqueio");
        var admin = await LoginAdminAsync(adminClient);

        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var bloquear = await adminClient.PutAsync($"/api/admin/usuarios/{cliente.Auth.UsuarioId}/bloquear", null);
        Assert.Equal(HttpStatusCode.OK, bloquear.StatusCode);

        var desbloquear = await adminClient.PutAsync($"/api/admin/usuarios/{cliente.Auth.UsuarioId}/desbloquear", null);
        var desbloqueado = await desbloquear.Content.ReadFromJsonAsync<UsuarioAdminDetalheResponse>();

        Assert.Equal(HttpStatusCode.OK, desbloquear.StatusCode);
        Assert.NotNull(desbloqueado);
        Assert.True(desbloqueado!.Ativo);

        var login = await clienteClient.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = cliente.Email,
            Senha = cliente.Senha
        });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task BloquearProprioAdministrador_DeveRetornar400()
    {
        using var adminClient = _factory.CreateClient();

        var admin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var response = await adminClient.PutAsync($"/api/admin/usuarios/{admin.UsuarioId}/bloquear", null);
        var payload = await response.Content.ReadFromJsonAsync<MeAjudaAi.Application.DTOs.Common.MensagemErroResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("Não é permitido bloquear o próprio usuário administrador.", payload!.Mensagem);
    }

    private static async Task<UsuarioRegistrado> RegistrarUsuarioAsync(HttpClient client, TipoPerfil tipoPerfil, string prefixo)
    {
        var email = $"{prefixo}-{Guid.NewGuid():N}@teste.local";
        const string senha = "Senha@123";

        var response = await client.PostAsJsonAsync("/api/auth/registrar", new RegistrarUsuarioRequest
        {
            Nome = $"{prefixo} teste",
            Email = email,
            Telefone = "11999999999",
            Senha = senha,
            TipoPerfil = tipoPerfil
        });

        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);

        return new UsuarioRegistrado(auth!, email, senha);
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

    private sealed record UsuarioRegistrado(AuthResponse Auth, string Email, string Senha);
}
