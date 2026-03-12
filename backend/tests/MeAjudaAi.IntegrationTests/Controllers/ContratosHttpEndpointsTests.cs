using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Application.DTOs.Cidades;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.IntegrationTests.Infrastructure;

namespace MeAjudaAi.IntegrationTests.Controllers;

[Trait("Category", "ApiContract")]
public class ContratosHttpEndpointsTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ContratosHttpEndpointsTests(TestWebApplicationFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ObterDetalhesProfissional_DeveRetornarMensagemErroQuandoProfissionalNaoExistir()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/profissionais/{Guid.NewGuid()}/detalhes");
        var payload = await response.Content.ReadFromJsonAsync<MensagemErroResponse>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(payload);
    }

    [Fact]
    public async Task UploadPortfolio_DeveRetornarMensagemErroQuandoArquivoNaoForEnviado()
    {
        using var client = _factory.CreateClient();

        var auth = await RegistrarUsuarioAsync(client, TipoPerfil.Profissional, "contrato-upload");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Token);

        var multipart = new MultipartFormDataContent();
        var arquivoVazio = new ByteArrayContent(Array.Empty<byte>());
        arquivoVazio.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        multipart.Add(arquivoVazio, "arquivo", "vazio.jpg");

        var response = await client.PostAsync("/api/profissionais/me/upload-portfolio", multipart);
        var payload = await response.Content.ReadFromJsonAsync<MensagemErroResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Arquivo é obrigatório.", payload?.Mensagem);
    }

    [Fact]
    public async Task ImportarGeografia_DeveRetornarContratoTipadoQuandoUsuarioForAdministrador()
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
        var payload = await response.Content.ReadFromJsonAsync<ImportacaoGeografiaResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("Importação concluída.", payload!.Mensagem);
        Assert.True(payload.Estados > 0);
        Assert.True(payload.Cidades > 0);
        Assert.True(payload.Bairros > 0);
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
