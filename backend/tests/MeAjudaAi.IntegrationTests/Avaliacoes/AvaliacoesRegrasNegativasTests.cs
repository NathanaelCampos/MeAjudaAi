using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Application.DTOs.Avaliacoes;
using MeAjudaAi.Application.DTOs.Servicos;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.IntegrationTests.Infrastructure;

namespace MeAjudaAi.IntegrationTests.Avaliacoes;

public class AvaliacoesRegrasNegativasTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AvaliacoesRegrasNegativasTests(TestWebApplicationFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CriarAvaliacao_DeveRetornarBadRequestQuandoServicoJaTiverSidoAvaliado()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-avaliacao-unica");
        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional-avaliacao-unica");

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
            Titulo = "Troca de disjuntor",
            Descricao = "Substituir disjuntor do quadro",
            ValorCombinado = 180m
        });

        var servico = await criarServicoResponse.Content.ReadFromJsonAsync<ServicoResponse>();

        Assert.Equal(HttpStatusCode.OK, criarServicoResponse.StatusCode);
        Assert.NotNull(servico);

        await profissionalClient.PutAsync($"/api/servicos/{servico!.Id}/aceitar", null);
        await profissionalClient.PutAsync($"/api/servicos/{servico.Id}/iniciar", null);
        await profissionalClient.PutAsync($"/api/servicos/{servico.Id}/concluir", null);

        var primeiraAvaliacaoResponse = await clienteClient.PostAsJsonAsync("/api/avaliacoes", new CriarAvaliacaoRequest
        {
            ServicoId = servico.Id,
            NotaAtendimento = NotaAtendimento.Excelente,
            NotaServico = NotaServico.Excelente,
            NotaPreco = NotaPreco.BomCustoBeneficio,
            Comentario = "Primeira avaliacao"
        });

        Assert.Equal(HttpStatusCode.OK, primeiraAvaliacaoResponse.StatusCode);

        var segundaAvaliacaoResponse = await clienteClient.PostAsJsonAsync("/api/avaliacoes", new CriarAvaliacaoRequest
        {
            ServicoId = servico.Id,
            NotaAtendimento = NotaAtendimento.Bom,
            NotaServico = NotaServico.Bom,
            NotaPreco = NotaPreco.Justo,
            Comentario = "Segunda avaliacao"
        });

        var payload = await segundaAvaliacaoResponse.Content.ReadFromJsonAsync<MensagemErroResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, segundaAvaliacaoResponse.StatusCode);
        Assert.Equal("Este serviço já foi avaliado.", payload?.Mensagem);
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
