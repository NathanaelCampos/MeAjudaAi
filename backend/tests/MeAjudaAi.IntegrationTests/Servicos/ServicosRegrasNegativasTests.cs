using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Application.DTOs.Avaliacoes;
using MeAjudaAi.Application.DTOs.Servicos;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.IntegrationTests.Infrastructure;

namespace MeAjudaAi.IntegrationTests.Servicos;

public class ServicosRegrasNegativasTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ServicosRegrasNegativasTests(TestWebApplicationFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CriarAvaliacao_DeveRetornarBadRequestQuandoServicoNaoEstiverConcluido()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-avaliacao");
        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional-avaliacao");

        clienteClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authCliente.Token);

        profissionalClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authProfissional.Token);

        var cidadeId = await _factory.ObterCidadeIdAsync();
        var profissionalId = await _factory.ObterProfissionalIdPorUsuarioIdAsync(authProfissional.UsuarioId);

        var criarServicoResponse = await clienteClient.PostAsJsonAsync("/api/servicos", new CriarServicoRequest
        {
            ProfissionalId = profissionalId,
            CidadeId = cidadeId,
            Titulo = "Instalacao de lampada",
            Descricao = "Trocar lampada da sala",
            ValorCombinado = 50m
        });

        var servico = await criarServicoResponse.Content.ReadFromJsonAsync<ServicoResponse>();

        Assert.Equal(HttpStatusCode.OK, criarServicoResponse.StatusCode);
        Assert.NotNull(servico);
        Assert.Equal(StatusServico.Solicitado, servico!.Status);

        var criarAvaliacaoResponse = await clienteClient.PostAsJsonAsync("/api/avaliacoes", new CriarAvaliacaoRequest
        {
            ServicoId = servico.Id,
            NotaAtendimento = NotaAtendimento.Bom,
            NotaServico = NotaServico.Bom,
            NotaPreco = NotaPreco.Justo,
            Comentario = "Ainda nao concluiu"
        });

        var payload = await criarAvaliacaoResponse.Content.ReadFromJsonAsync<MensagemErroResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, criarAvaliacaoResponse.StatusCode);
        Assert.Equal("Somente serviços concluídos podem ser avaliados.", payload?.Mensagem);
    }

    [Fact]
    public async Task ObterPorId_DeveRetornarBadRequestQuandoUsuarioNaoParticipaDoServico()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var intrusoClient = _factory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-servico");
        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional-servico");
        var authIntruso = await RegistrarUsuarioAsync(intrusoClient, TipoPerfil.Cliente, "intruso-servico");

        clienteClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authCliente.Token);

        intrusoClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authIntruso.Token);

        var cidadeId = await _factory.ObterCidadeIdAsync();
        var profissionalId = await _factory.ObterProfissionalIdPorUsuarioIdAsync(authProfissional.UsuarioId);

        var criarServicoResponse = await clienteClient.PostAsJsonAsync("/api/servicos", new CriarServicoRequest
        {
            ProfissionalId = profissionalId,
            CidadeId = cidadeId,
            Titulo = "Reparo de torneira",
            Descricao = "Torneira vazando",
            ValorCombinado = 90m
        });

        var servico = await criarServicoResponse.Content.ReadFromJsonAsync<ServicoResponse>();

        Assert.Equal(HttpStatusCode.OK, criarServicoResponse.StatusCode);
        Assert.NotNull(servico);

        var obterResponse = await intrusoClient.GetAsync($"/api/servicos/{servico!.Id}");
        var payload = await obterResponse.Content.ReadFromJsonAsync<MensagemErroResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, obterResponse.StatusCode);
        Assert.Equal("Você não pode acessar este serviço.", payload?.Mensagem);
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
