using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.DTOs.Impulsionamentos;
using MeAjudaAi.Application.DTOs.Profissionais;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.IntegrationTests.Infrastructure;

namespace MeAjudaAi.IntegrationTests.Profissionais;

public class ProfissionaisBuscaTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ProfissionaisBuscaTests(TestWebApplicationFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Buscar_DeveFiltrarPorProfissaoCidadeEBairro()
    {
        using var profissionalAlvoClient = _factory.CreateClient();
        using var profissionalOutroClient = _factory.CreateClient();
        using var client = _factory.CreateClient();

        var authAlvo = await RegistrarProfissionalAsync(profissionalAlvoClient, "profissional-filtro-alvo", "Filtro Carlos Eletricista");
        var authOutro = await RegistrarProfissionalAsync(profissionalOutroClient, "profissional-filtro-outro", "Filtro Bruno Outro");

        var cidadeId = await _factory.ObterCidadeIdAsync();
        var bairroId = await _factory.ObterBairroIdAsync();
        var (profissaoId, especialidadeId) = await _factory.ObterProfissaoEEspecialidadeAsync();

        await ConfigurarProfissionalAsync(profissionalAlvoClient, authAlvo.Token, profissaoId, especialidadeId, cidadeId, bairroId, true);
        await AtualizarNomeExibicaoAsync(profissionalAlvoClient, authAlvo.Token, "Filtro Carlos Eletricista");

        await AtualizarNomeExibicaoAsync(profissionalOutroClient, authOutro.Token, "Filtro Bruno Outro");

        var response = await client.GetFromJsonAsync<PaginacaoResponse<ProfissionalResumoResponse>>(
            $"/api/profissionais/buscar?profissaoId={profissaoId}&cidadeId={cidadeId}&bairroId={bairroId}&pagina=1&tamanhoPagina=10");

        Assert.NotNull(response);
        Assert.Single(response!.Itens);
        Assert.Equal("Filtro Carlos Eletricista", response.Itens[0].NomeExibicao);
        Assert.Equal(1, response.TotalRegistros);
    }

    [Fact]
    public async Task Buscar_DeveAplicarPaginacaoComOrdenacaoPorNome()
    {
        using var profissional1Client = _factory.CreateClient();
        using var profissional2Client = _factory.CreateClient();
        using var profissional3Client = _factory.CreateClient();
        using var client = _factory.CreateClient();

        var cidadeId = await _factory.ObterCidadeIdAsync();
        var bairroId = await _factory.ObterBairroIdAsync();
        var (profissaoId, especialidadeId) = await _factory.ObterProfissaoEEspecialidadeAsync();

        var auth1 = await RegistrarProfissionalAsync(profissional1Client, "paginacao-1", "Paginacao AA");
        var auth2 = await RegistrarProfissionalAsync(profissional2Client, "paginacao-2", "Paginacao BB");
        var auth3 = await RegistrarProfissionalAsync(profissional3Client, "paginacao-3", "Paginacao CC");

        await ConfigurarProfissionalAsync(profissional1Client, auth1.Token, profissaoId, especialidadeId, cidadeId, bairroId, false);
        await ConfigurarProfissionalAsync(profissional2Client, auth2.Token, profissaoId, especialidadeId, cidadeId, bairroId, false);
        await ConfigurarProfissionalAsync(profissional3Client, auth3.Token, profissaoId, especialidadeId, cidadeId, bairroId, false);

        await AtualizarNomeExibicaoAsync(profissional1Client, auth1.Token, "Paginacao AA");
        await AtualizarNomeExibicaoAsync(profissional2Client, auth2.Token, "Paginacao BB");
        await AtualizarNomeExibicaoAsync(profissional3Client, auth3.Token, "Paginacao CC");

        var response = await client.GetFromJsonAsync<PaginacaoResponse<ProfissionalResumoResponse>>(
            "/api/profissionais/buscar?nome=Paginacao&ordenacao=NomeAsc&pagina=2&tamanhoPagina=1");

        Assert.NotNull(response);
        Assert.Equal(2, response!.PaginaAtual);
        Assert.Equal(1, response.TamanhoPagina);
        Assert.Equal(3, response.TotalRegistros);
        Assert.Equal(3, response.TotalPaginas);
        Assert.Single(response.Itens);
        Assert.Equal("Paginacao BB", response.Itens[0].NomeExibicao);
    }

    [Fact]
    public async Task Buscar_DevePriorizarProfissionalImpulsionadoNaOrdenacaoPorRelevancia()
    {
        using var impulsionadoClient = _factory.CreateClient();
        using var comumClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();
        using var client = _factory.CreateClient();

        var cidadeId = await _factory.ObterCidadeIdAsync();
        var bairroId = await _factory.ObterBairroIdAsync();
        var (profissaoId, especialidadeId) = await _factory.ObterProfissaoEEspecialidadeAsync();

        var authImpulsionado = await RegistrarProfissionalAsync(impulsionadoClient, "impulsionado", "Prioridade Zeta");
        var authComum = await RegistrarProfissionalAsync(comumClient, "comum", "Prioridade Alfa");
        var authAdmin = await LoginAdminAsync(adminClient);

        await ConfigurarProfissionalAsync(impulsionadoClient, authImpulsionado.Token, profissaoId, especialidadeId, cidadeId, bairroId, false);
        await ConfigurarProfissionalAsync(comumClient, authComum.Token, profissaoId, especialidadeId, cidadeId, bairroId, false);

        await AtualizarNomeExibicaoAsync(impulsionadoClient, authImpulsionado.Token, "Prioridade Zeta");
        await AtualizarNomeExibicaoAsync(comumClient, authComum.Token, "Prioridade Alfa");

        var planos = await client.GetFromJsonAsync<List<PlanoImpulsionamentoResponse>>("/api/impulsionamentos/planos");
        Assert.NotNull(planos);

        impulsionadoClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authImpulsionado.Token);

        var contratarResponse = await impulsionadoClient.PostAsJsonAsync("/api/impulsionamentos/contratar", new ContratarPlanoImpulsionamentoRequest
        {
            PlanoImpulsionamentoId = planos![0].Id,
            CodigoReferenciaPagamento = "busca-prioridade-001"
        });

        Assert.Equal(HttpStatusCode.OK, contratarResponse.StatusCode);
        var contratado = await contratarResponse.Content.ReadFromJsonAsync<ImpulsionamentoProfissionalResponse>();
        Assert.NotNull(contratado);

        adminClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authAdmin.Token);

        var confirmarResponse = await adminClient.PutAsync($"/api/impulsionamentos/{contratado!.Id}/confirmar-pagamento", null);
        Assert.Equal(HttpStatusCode.OK, confirmarResponse.StatusCode);

        var response = await client.GetFromJsonAsync<PaginacaoResponse<ProfissionalResumoResponse>>(
            $"/api/profissionais/buscar?nome=Prioridade&profissaoId={profissaoId}&ordenacao=Relevancia&pagina=1&tamanhoPagina=10");

        Assert.NotNull(response);
        Assert.True(response!.Itens.Count >= 2);
        Assert.Equal("Prioridade Zeta", response.Itens[0].NomeExibicao);
        Assert.True(response.Itens[0].EstaImpulsionado);
    }

    private static async Task<AuthResponse> RegistrarProfissionalAsync(HttpClient client, string prefixo, string nome)
    {
        var response = await client.PostAsJsonAsync("/api/auth/registrar", new RegistrarUsuarioRequest
        {
            Nome = nome,
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

    private static async Task AtualizarNomeExibicaoAsync(HttpClient client, string token, string nomeExibicao)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PutAsJsonAsync("/api/profissionais/me", new AtualizarProfissionalRequest
        {
            NomeExibicao = nomeExibicao,
            Descricao = "Descricao profissional",
            WhatsApp = "11999999999",
            Instagram = string.Empty,
            Facebook = string.Empty,
            OutraFormaContato = string.Empty,
            AceitaContatoPeloApp = true
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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

    private static async Task ConfigurarProfissionalAsync(
        HttpClient client,
        string token,
        Guid profissaoId,
        Guid especialidadeId,
        Guid cidadeId,
        Guid bairroId,
        bool cidadeInteira)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var profissaoResponse = await client.PutAsJsonAsync("/api/profissionais/me/profissoes", new AtualizarProfissoesProfissionalRequest
        {
            ProfissaoIds = [profissaoId]
        });

        Assert.Equal(HttpStatusCode.NoContent, profissaoResponse.StatusCode);

        var especialidadeResponse = await client.PutAsJsonAsync("/api/profissionais/me/especialidades", new AtualizarEspecialidadesProfissionalRequest
        {
            EspecialidadeIds = [especialidadeId]
        });

        Assert.Equal(HttpStatusCode.NoContent, especialidadeResponse.StatusCode);

        var areaResponse = await client.PutAsJsonAsync("/api/profissionais/me/areas-atendimento", new AtualizarAreasAtendimentoRequest
        {
            Areas =
            [
                new AreaAtendimentoItemRequest
                {
                    CidadeId = cidadeId,
                    BairroId = cidadeInteira ? null : bairroId,
                    CidadeInteira = cidadeInteira
                }
            ]
        });

        Assert.Equal(HttpStatusCode.NoContent, areaResponse.StatusCode);
    }
}
