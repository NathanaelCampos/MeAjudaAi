using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.IntegrationTests.Infrastructure;

namespace MeAjudaAi.IntegrationTests.Avaliacoes;

public class AvaliacoesAutorizacaoEModeracaoTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AvaliacoesAutorizacaoEModeracaoTests(TestWebApplicationFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ListarPendentes_DeveRetornarForbiddenQuandoUsuarioNaoForAdministrador()
    {
        using var client = _factory.CreateClient();

        var auth = await RegistrarUsuarioAsync(client, TipoPerfil.Cliente, "cliente-admin");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Token);

        var response = await client.GetAsync("/api/avaliacoes/pendentes");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ListarPendentes_DeveRetornarUnauthorizedQuandoUsuarioNaoEstiverAutenticado()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/avaliacoes/pendentes");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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
