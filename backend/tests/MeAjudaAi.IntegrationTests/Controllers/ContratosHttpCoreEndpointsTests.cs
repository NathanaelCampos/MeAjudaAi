using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Application.DTOs.Cidades;
using MeAjudaAi.Application.DTOs.Impulsionamentos;
using MeAjudaAi.Application.DTOs.Profissoes;
using MeAjudaAi.Application.DTOs.Servicos;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.IntegrationTests.Infrastructure;

namespace MeAjudaAi.IntegrationTests.Controllers;

[Trait("Category", "ApiContract")]
public class ContratosHttpCoreEndpointsTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ContratosHttpCoreEndpointsTests(TestWebApplicationFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_DeveRetornarMensagemErroTipadaQuandoCredenciaisForemInvalidas()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "naoexiste@teste.local",
            Senha = "SenhaErrada@123"
        });

        var payload = await response.Content.ReadFromJsonAsync<MensagemErroResponse>();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("E-mail ou senha inválidos.", payload?.Mensagem);
    }

    [Fact]
    public async Task ListarProfissoesECidades_DeveRetornarContratosTipados()
    {
        using var client = _factory.CreateClient();

        var responseProfissoes = await client.GetAsync("/api/profissoes");
        var responseCidades = await client.GetAsync("/api/cidades");

        var profissoes = await responseProfissoes.Content.ReadFromJsonAsync<List<ProfissaoResponse>>();
        var cidades = await responseCidades.Content.ReadFromJsonAsync<List<CidadeResponse>>();

        Assert.Equal(HttpStatusCode.OK, responseProfissoes.StatusCode);
        Assert.NotNull(profissoes);
        Assert.NotEmpty(profissoes!);
        Assert.All(profissoes!, x => Assert.False(string.IsNullOrWhiteSpace(x.Nome)));

        Assert.Equal(HttpStatusCode.OK, responseCidades.StatusCode);
        Assert.NotNull(cidades);
        Assert.NotEmpty(cidades!);
        Assert.All(cidades!, x => Assert.False(string.IsNullOrWhiteSpace(x.Nome)));
    }

    [Fact]
    public async Task ConfirmarPagamentoPorCodigo_DeveRetornarErroValidacaoTipadoQuandoCodigoNaoForInformado()
    {
        using var client = _factory.CreateClient();

        var adminAuth = await FazerLoginAdminAsync(client);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", adminAuth.Token);

        var response = await client.PostAsJsonAsync("/api/impulsionamentos/confirmar-pagamento", new ConfirmarPagamentoImpulsionamentoRequest
        {
            CodigoReferenciaPagamento = string.Empty
        });

        var payload = await response.Content.ReadFromJsonAsync<ErroValidacaoResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("Erro de validação.", payload!.Mensagem);
        Assert.Contains(payload.Erros, x => x.Campo == "CodigoReferenciaPagamento");
    }

    [Fact]
    public async Task CriarAvaliacao_DeveRetornarMensagemErroTipadaQuandoServicoNaoEstiverConcluido()
    {
        using var client = _factory.CreateClient();

        var clienteAuth = await RegistrarUsuarioAsync(client, TipoPerfil.Cliente, "contrato-cliente");
        var profissionalAuth = await RegistrarUsuarioAsync(client, TipoPerfil.Profissional, "contrato-profissional");
        var profissionalId = await _factory.ObterProfissionalIdPorUsuarioIdAsync(profissionalAuth.UsuarioId);
        var cidadeId = await _factory.ObterCidadeIdAsync();
        var bairroId = await _factory.ObterBairroIdAsync();
        var (profissaoId, especialidadeId) = await _factory.ObterProfissaoEEspecialidadeAsync();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", clienteAuth.Token);

        var responseServico = await client.PostAsJsonAsync("/api/servicos", new CriarServicoRequest
        {
            ProfissionalId = profissionalId,
            ProfissaoId = profissaoId,
            EspecialidadeId = especialidadeId,
            CidadeId = cidadeId,
            BairroId = bairroId,
            Titulo = "Servico para contrato",
            Descricao = "Servico ainda nao concluido",
            ValorCombinado = 100m
        });

        responseServico.EnsureSuccessStatusCode();
        var servico = await responseServico.Content.ReadFromJsonAsync<ServicoResponse>();
        Assert.NotNull(servico);

        var responseAvaliacao = await client.PostAsJsonAsync("/api/avaliacoes", new
        {
            servicoId = servico!.Id,
            notaAtendimento = 5,
            notaServico = 5,
            notaPreco = 5,
            comentario = "teste"
        });

        var payload = await responseAvaliacao.Content.ReadFromJsonAsync<MensagemErroResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, responseAvaliacao.StatusCode);
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload!.Mensagem));
    }

    private static async Task<AuthResponse> FazerLoginAdminAsync(HttpClient client)
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
