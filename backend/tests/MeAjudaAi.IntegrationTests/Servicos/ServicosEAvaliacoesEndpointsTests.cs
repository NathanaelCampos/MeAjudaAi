using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Application.DTOs.Avaliacoes;
using MeAjudaAi.Application.DTOs.Servicos;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.IntegrationTests.Infrastructure;

namespace MeAjudaAi.IntegrationTests.Servicos;

public class ServicosEAvaliacoesEndpointsTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ServicosEAvaliacoesEndpointsTests(TestWebApplicationFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task FluxoCompleto_DeveCriarConcluirAvaliarEModerarServico()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente");
        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional");
        var authAdmin = await LoginAdminAsync(adminClient);

        clienteClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authCliente.Token);

        profissionalClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authProfissional.Token);

        adminClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authAdmin.Token);

        var cidadeId = await _factory.ObterCidadeIdAsync();
        var profissionalId = await _factory.ObterProfissionalIdPorUsuarioIdAsync(authProfissional.UsuarioId);

        var criarServicoResponse = await clienteClient.PostAsJsonAsync("/api/servicos", new CriarServicoRequest
        {
            ProfissionalId = profissionalId,
            CidadeId = cidadeId,
            Titulo = "Troca de tomada",
            Descricao = "Trocar tomada da cozinha",
            ValorCombinado = 120m
        });

        var servico = await criarServicoResponse.Content.ReadFromJsonAsync<ServicoResponse>();

        Assert.Equal(HttpStatusCode.OK, criarServicoResponse.StatusCode);
        Assert.NotNull(servico);
        Assert.Equal(StatusServico.Solicitado, servico!.Status);

        var aceitarResponse = await profissionalClient.PutAsync($"/api/servicos/{servico.Id}/aceitar", null);
        var aceito = await aceitarResponse.Content.ReadFromJsonAsync<ServicoResponse>();
        Assert.Equal(HttpStatusCode.OK, aceitarResponse.StatusCode);
        Assert.NotNull(aceito);
        Assert.Equal(StatusServico.Aceito, aceito!.Status);

        var iniciarResponse = await profissionalClient.PutAsync($"/api/servicos/{servico.Id}/iniciar", null);
        var iniciado = await iniciarResponse.Content.ReadFromJsonAsync<ServicoResponse>();
        Assert.Equal(HttpStatusCode.OK, iniciarResponse.StatusCode);
        Assert.NotNull(iniciado);
        Assert.Equal(StatusServico.EmExecucao, iniciado!.Status);

        var concluirResponse = await profissionalClient.PutAsync($"/api/servicos/{servico.Id}/concluir", null);
        var concluido = await concluirResponse.Content.ReadFromJsonAsync<ServicoResponse>();
        Assert.Equal(HttpStatusCode.OK, concluirResponse.StatusCode);
        Assert.NotNull(concluido);
        Assert.Equal(StatusServico.Concluido, concluido!.Status);

        var criarAvaliacaoResponse = await clienteClient.PostAsJsonAsync("/api/avaliacoes", new CriarAvaliacaoRequest
        {
            ServicoId = servico.Id,
            NotaAtendimento = NotaAtendimento.Excelente,
            NotaServico = NotaServico.Bom,
            NotaPreco = NotaPreco.Justo,
            Comentario = "Servico concluido com qualidade"
        });

        var avaliacao = await criarAvaliacaoResponse.Content.ReadFromJsonAsync<AvaliacaoResponse>();

        Assert.Equal(HttpStatusCode.OK, criarAvaliacaoResponse.StatusCode);
        Assert.NotNull(avaliacao);
        Assert.Equal(StatusModeracaoComentario.Pendente, avaliacao!.StatusModeracaoComentario);

        var pendentes = await adminClient.GetFromJsonAsync<List<AvaliacaoResponse>>("/api/avaliacoes/pendentes");
        Assert.NotNull(pendentes);
        Assert.Contains(pendentes!, x => x.Id == avaliacao.Id);

        var moderarResponse = await adminClient.PutAsJsonAsync(
            $"/api/avaliacoes/{avaliacao.Id}/moderar",
            new ModerarAvaliacaoRequest
            {
                Acao = AcaoModeracaoAvaliacao.Aprovar
            });

        var moderada = await moderarResponse.Content.ReadFromJsonAsync<AvaliacaoResponse>();

        Assert.Equal(HttpStatusCode.OK, moderarResponse.StatusCode);
        Assert.NotNull(moderada);
        Assert.Equal(StatusModeracaoComentario.Aprovado, moderada!.StatusModeracaoComentario);

        var publicas = await clienteClient.GetFromJsonAsync<List<AvaliacaoResponse>>($"/api/avaliacoes/profissional/{profissionalId}");
        Assert.NotNull(publicas);
        Assert.Contains(publicas!, x => x.Id == avaliacao.Id && x.StatusModeracaoComentario == StatusModeracaoComentario.Aprovado);
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
}
