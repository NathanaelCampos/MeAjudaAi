using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Application.DTOs.Profissionais;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.IntegrationTests.Infrastructure;

namespace MeAjudaAi.IntegrationTests.Profissionais;

public class ProfissionaisPortfolioERecebimentoTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ProfissionaisPortfolioERecebimentoTests(TestWebApplicationFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UploadEAtualizacaoPortfolio_DeveListarFotosERemoverArquivoOrfao()
    {
        using var client = _factory.CreateClient();

        var auth = await RegistrarProfissionalAsync(client, "portfolio");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Token);

        var upload1 = await UploadImagemAsync(client, "foto1.jpg");
        var upload2 = await UploadImagemAsync(client, "foto2.jpg");

        Assert.StartsWith("http://localhost/uploads/portfolio/", upload1.UrlArquivo, StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith("http://localhost/uploads/portfolio/", upload2.UrlArquivo, StringComparison.OrdinalIgnoreCase);

        var arquivo1Relativo = new Uri(upload1.UrlArquivo).AbsolutePath;
        var arquivo2Relativo = new Uri(upload2.UrlArquivo).AbsolutePath;

        await AtualizarPortfolioAsync(client, new AtualizarPortfolioRequest
        {
            Fotos =
            [
                new PortfolioFotoRequest
                {
                    UrlArquivo = arquivo1Relativo,
                    Legenda = "Primeira foto",
                    Ordem = 2
                },
                new PortfolioFotoRequest
                {
                    UrlArquivo = arquivo2Relativo,
                    Legenda = "Segunda foto",
                    Ordem = 1
                }
            ]
        });

        var profissionalId = await _factory.ObterProfissionalIdPorUsuarioIdAsync(auth.UsuarioId);
        var portfolioInicial = await client.GetFromJsonAsync<List<PortfolioFotoResponse>>($"/api/profissionais/{profissionalId}/portfolio");

        Assert.NotNull(portfolioInicial);
        Assert.Equal(2, portfolioInicial!.Count);
        Assert.Equal("Segunda foto", portfolioInicial[0].Legenda);
        Assert.Equal("Primeira foto", portfolioInicial[1].Legenda);

        await AtualizarPortfolioAsync(client, new AtualizarPortfolioRequest
        {
            Fotos =
            [
                new PortfolioFotoRequest
                {
                    UrlArquivo = arquivo2Relativo,
                    Legenda = "Segunda foto mantida",
                    Ordem = 1
                }
            ]
        });

        var portfolioFinal = await client.GetFromJsonAsync<List<PortfolioFotoResponse>>($"/api/profissionais/{profissionalId}/portfolio");
        Assert.NotNull(portfolioFinal);
        Assert.Single(portfolioFinal!);
        Assert.Equal("Segunda foto mantida", portfolioFinal[0].Legenda);

        var caminhoArquivo1 = Path.Combine(
            _factory.ContentRootPath,
            "Uploads",
            arquivo1Relativo.Replace("/uploads/", string.Empty, StringComparison.OrdinalIgnoreCase)
                .TrimStart('/')
                .Replace('/', Path.DirectorySeparatorChar));
        var caminhoArquivo2 = Path.Combine(
            _factory.ContentRootPath,
            "Uploads",
            arquivo2Relativo.Replace("/uploads/", string.Empty, StringComparison.OrdinalIgnoreCase)
                .TrimStart('/')
                .Replace('/', Path.DirectorySeparatorChar));

        Assert.False(File.Exists(caminhoArquivo1));
        Assert.True(File.Exists(caminhoArquivo2));
    }

    [Fact]
    public async Task AtualizarFormasRecebimento_DeveDeduplicarPorTipoEExporNosDetalhes()
    {
        using var client = _factory.CreateClient();

        var auth = await RegistrarProfissionalAsync(client, "recebimento");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Token);

        var response = await client.PutAsJsonAsync("/api/profissionais/me/formas-recebimento", new AtualizarFormasRecebimentoRequest
        {
            Itens =
            [
                new FormaRecebimentoRequest
                {
                    TipoFormaRecebimento = TipoFormaRecebimento.Pix,
                    Descricao = "chave-pix-1"
                },
                new FormaRecebimentoRequest
                {
                    TipoFormaRecebimento = TipoFormaRecebimento.Pix,
                    Descricao = "chave-pix-2"
                },
                new FormaRecebimentoRequest
                {
                    TipoFormaRecebimento = TipoFormaRecebimento.Dinheiro,
                    Descricao = "Dinheiro na entrega"
                }
            ]
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var profissionalId = await _factory.ObterProfissionalIdPorUsuarioIdAsync(auth.UsuarioId);

        var formas = await client.GetFromJsonAsync<List<FormaRecebimentoResponse>>($"/api/profissionais/{profissionalId}/formas-recebimento");
        Assert.NotNull(formas);
        Assert.Equal(2, formas!.Count);
        Assert.Contains(formas, x => x.TipoFormaRecebimento == TipoFormaRecebimento.Pix && x.Descricao == "chave-pix-1");
        Assert.Contains(formas, x => x.TipoFormaRecebimento == TipoFormaRecebimento.Dinheiro);

        var detalhes = await client.GetFromJsonAsync<ProfissionalDetalhesResponse>($"/api/profissionais/{profissionalId}/detalhes");
        Assert.NotNull(detalhes);
        Assert.Equal(2, detalhes!.FormasRecebimento.Count);
    }

    [Fact]
    public async Task UploadPortfolio_DeveRetornarBadRequestQuandoContentTypeForInvalido()
    {
        using var client = _factory.CreateClient();

        var auth = await RegistrarProfissionalAsync(client, "upload-invalido");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Token);

        using var form = new MultipartFormDataContent();
        using var arquivo = new ByteArrayContent("arquivo invalido"u8.ToArray());
        arquivo.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        form.Add(arquivo, "arquivo", "arquivo.txt");

        var response = await client.PostAsync("/api/profissionais/me/upload-portfolio", form);
        var payload = await response.Content.ReadFromJsonAsync<MensagemErroResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Formato de arquivo não permitido. Use jpg, jpeg, png ou webp.", payload?.Mensagem);
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

    private static async Task<UploadPortfolioResponse> UploadImagemAsync(HttpClient client, string nomeArquivo)
    {
        using var form = new MultipartFormDataContent();
        using var arquivo = new ByteArrayContent([0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46]);
        arquivo.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        form.Add(arquivo, "arquivo", nomeArquivo);

        var response = await client.PostAsync("/api/profissionais/me/upload-portfolio", form);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<UploadPortfolioResponse>();
        Assert.NotNull(payload);

        return payload!;
    }

    private static async Task AtualizarPortfolioAsync(HttpClient client, AtualizarPortfolioRequest request)
    {
        var response = await client.PutAsJsonAsync("/api/profissionais/me/portfolio", request);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
