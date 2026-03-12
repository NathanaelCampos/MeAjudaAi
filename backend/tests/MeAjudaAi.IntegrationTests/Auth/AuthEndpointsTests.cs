using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.IntegrationTests.Infrastructure;

namespace MeAjudaAi.IntegrationTests.Auth;

public class AuthEndpointsTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(TestWebApplicationFactory factory)
        : base(factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Registrar_DeveRetornarBadRequestQuandoTipoPerfilForAdministrador()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/registrar", new RegistrarUsuarioRequest
        {
            Nome = "Admin Indevido",
            Email = "admin.publico@teste.local",
            Telefone = "11999999999",
            Senha = "Admin@123",
            TipoPerfil = TipoPerfil.Administrador
        });

        var payload = await response.Content.ReadFromJsonAsync<MensagemErroResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Não é permitido criar administrador por este endpoint.", payload?.Mensagem);
    }

    [Fact]
    public async Task Login_DeveRetornarUnauthorizedQuandoCredenciaisForemInvalidas()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "naoexiste@teste.local",
            Senha = "SenhaErrada@123"
        });

        var payload = await response.Content.ReadFromJsonAsync<MensagemErroResponse>();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("E-mail ou senha inválidos.", payload?.Mensagem);
    }
}
