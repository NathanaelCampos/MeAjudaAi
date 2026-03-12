using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.IntegrationTests.Infrastructure;
using System.Text.Json;

namespace MeAjudaAi.IntegrationTests.Importacao;

public class ImportacaoEndpointsTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ImportacaoEndpointsTests(TestWebApplicationFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ImportarGeografia_DeveRetornarUnauthorizedQuandoUsuarioNaoEstiverAutenticado()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/importacao/geografia", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ImportarGeografia_DeveRetornarForbiddenQuandoUsuarioNaoForAdministrador()
    {
        using var client = _factory.CreateClient();

        var auth = await RegistrarUsuarioAsync(client, TipoPerfil.Profissional, "importacao-proibida");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Token);

        var response = await client.PostAsync("/api/importacao/geografia", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ImportarGeografia_DevePermitirAcessoQuandoUsuarioForAdministrador()
    {
        using var client = _factory.CreateClient();

        var responseLogin = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = TestWebApplicationFactory.EmailAdmin,
            Senha = TestWebApplicationFactory.SenhaAdmin
        });

        responseLogin.EnsureSuccessStatusCode();
        var auth = await responseLogin.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.Token);

        var response = await client.PostAsync("/api/importacao/geografia", null);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Importação concluída.", json.RootElement.GetProperty("mensagem").GetString());
    }

    private static async Task<AuthResponse> RegistrarUsuarioAsync(
        HttpClient client,
        TipoPerfil tipoPerfil,
        string prefixo)
    {
        var response = await client.PostAsJsonAsync("/api/auth/registrar", new RegistrarUsuarioRequest
        {
            Nome = $"{prefixo} teste",
            Email = $"{prefixo}-{Guid.NewGuid():N}@teste.local",
            Telefone = "11999999999",
            Senha = "Senha@123",
            TipoPerfil = tipoPerfil
        });

        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);

        return auth!;
    }
}
