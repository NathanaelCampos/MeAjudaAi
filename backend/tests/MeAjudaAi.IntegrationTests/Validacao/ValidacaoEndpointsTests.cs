using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Application.DTOs.Profissionais;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.IntegrationTests.Infrastructure;

namespace MeAjudaAi.IntegrationTests.Validacao;

public class ValidacaoEndpointsTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ValidacaoEndpointsTests(TestWebApplicationFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Registrar_DeveRetornarEstruturaPadronizadaQuandoRequestForInvalido()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/registrar", new RegistrarUsuarioRequest
        {
            Nome = string.Empty,
            Email = "email-invalido",
            Telefone = new string('1', 21),
            Senha = "123",
            TipoPerfil = TipoPerfil.Cliente
        });

        var payload = await response.Content.ReadFromJsonAsync<ErroValidacaoResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("Erro de validação.", payload!.Mensagem);
        Assert.Contains(payload.Erros, x => x.Campo == "Nome");
        Assert.Contains(payload.Erros, x => x.Campo == "Email");
        Assert.Contains(payload.Erros, x => x.Campo == "Telefone");
        Assert.Contains(payload.Erros, x => x.Campo == "Senha");
    }

    [Fact]
    public async Task AtualizarPortfolio_DeveRetornarEstruturaPadronizadaQuandoFotoForInvalida()
    {
        using var client = _factory.CreateClient();

        var auth = await RegistrarProfissionalAsync(client, "validacao-portfolio");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Token);

        var response = await client.PutAsJsonAsync("/api/profissionais/me/portfolio", new AtualizarPortfolioRequest
        {
            Fotos =
            [
                new PortfolioFotoRequest
                {
                    UrlArquivo = string.Empty,
                    Legenda = new string('a', 301),
                    Ordem = -1
                }
            ]
        });

        var payload = await response.Content.ReadFromJsonAsync<ErroValidacaoResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("Erro de validação.", payload!.Mensagem);
        Assert.Contains(payload.Erros, x => x.Campo == "Fotos[0].UrlArquivo");
        Assert.Contains(payload.Erros, x => x.Campo == "Fotos[0].Legenda");
        Assert.Contains(payload.Erros, x => x.Campo == "Fotos[0].Ordem");
    }

    private static async Task<AuthResponse> RegistrarProfissionalAsync(HttpClient client, string prefixo)
    {
        var response = await client.PostAsJsonAsync("/api/auth/registrar", new RegistrarUsuarioRequest
        {
            Nome = $"{prefixo} teste",
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
}
