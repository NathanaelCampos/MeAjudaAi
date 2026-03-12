using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Application.DTOs.Impulsionamentos;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.IntegrationTests.Infrastructure;

namespace MeAjudaAi.IntegrationTests.Impulsionamentos;

public class ImpulsionamentosAutorizacaoTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ImpulsionamentosAutorizacaoTests(TestWebApplicationFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Contratar_DeveRetornarUnauthorizedQuandoUsuarioNaoEstiverAutenticado()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/impulsionamentos/contratar", new ContratarPlanoImpulsionamentoRequest
        {
            PlanoImpulsionamentoId = Guid.NewGuid()
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Contratar_DeveRetornarBadRequestQuandoUsuarioAutenticadoNaoForProfissional()
    {
        using var client = _factory.CreateClient();

        var auth = await RegistrarUsuarioAsync(client, TipoPerfil.Cliente, "cliente-impulsionamento");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Token);

        var planos = await client.GetFromJsonAsync<List<PlanoImpulsionamentoResponse>>("/api/impulsionamentos/planos");
        Assert.NotNull(planos);

        var response = await client.PostAsJsonAsync("/api/impulsionamentos/contratar", new ContratarPlanoImpulsionamentoRequest
        {
            PlanoImpulsionamentoId = planos![0].Id
        });

        var payload = await response.Content.ReadFromJsonAsync<MensagemErroResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Profissional não encontrado para o usuário autenticado.", payload?.Mensagem);
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
