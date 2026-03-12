using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Application.DTOs.Avaliacoes;
using MeAjudaAi.Application.DTOs.Impulsionamentos;
using MeAjudaAi.Application.DTOs.Notificacoes;
using MeAjudaAi.Application.DTOs.Servicos;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.IntegrationTests.Infrastructure;

namespace MeAjudaAi.IntegrationTests.Notificacoes;

public class NotificacoesEndpointsTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public NotificacoesEndpointsTests(TestWebApplicationFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task FluxoDeServicoEModeracao_DeveGerarNotificacoesParaProfissionalECliente()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-notif");
        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional-notif");
        var authAdmin = await LoginAdminAsync(adminClient);

        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authCliente.Token);
        profissionalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authProfissional.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authAdmin.Token);

        var cidadeId = await _factory.ObterCidadeIdAsync();
        var profissionalId = await _factory.ObterProfissionalIdPorUsuarioIdAsync(authProfissional.UsuarioId);

        var criarServicoResponse = await clienteClient.PostAsJsonAsync("/api/servicos", new CriarServicoRequest
        {
            ProfissionalId = profissionalId,
            CidadeId = cidadeId,
            Titulo = "Reparo de tomada",
            Descricao = "Tomada com mau contato",
            ValorCombinado = 90m
        });

        var servico = await criarServicoResponse.Content.ReadFromJsonAsync<ServicoResponse>();
        Assert.Equal(HttpStatusCode.OK, criarServicoResponse.StatusCode);
        Assert.NotNull(servico);

        var notificacoesProfissional = await profissionalClient.GetFromJsonAsync<List<NotificacaoResponse>>("/api/notificacoes/minhas");
        Assert.NotNull(notificacoesProfissional);
        Assert.Contains(notificacoesProfissional!, x => x.Tipo == TipoNotificacao.ServicoSolicitado && x.ReferenciaId == servico!.Id);

        var aceitarResponse = await profissionalClient.PutAsync($"/api/servicos/{servico!.Id}/aceitar", null);
        Assert.Equal(HttpStatusCode.OK, aceitarResponse.StatusCode);

        var notificacoesClienteAposAceite = await clienteClient.GetFromJsonAsync<List<NotificacaoResponse>>("/api/notificacoes/minhas");
        Assert.NotNull(notificacoesClienteAposAceite);
        Assert.Contains(notificacoesClienteAposAceite!, x => x.Tipo == TipoNotificacao.ServicoAceito && x.ReferenciaId == servico.Id);

        var concluirResponse = await profissionalClient.PutAsync($"/api/servicos/{servico.Id}/concluir", null);
        Assert.Equal(HttpStatusCode.OK, concluirResponse.StatusCode);

        var notificacoesClienteAposConclusao = await clienteClient.GetFromJsonAsync<List<NotificacaoResponse>>("/api/notificacoes/minhas");
        Assert.NotNull(notificacoesClienteAposConclusao);
        Assert.Contains(notificacoesClienteAposConclusao!, x => x.Tipo == TipoNotificacao.ServicoConcluido && x.ReferenciaId == servico.Id);

        var criarAvaliacaoResponse = await clienteClient.PostAsJsonAsync("/api/avaliacoes", new CriarAvaliacaoRequest
        {
            ServicoId = servico.Id,
            NotaAtendimento = NotaAtendimento.Excelente,
            NotaServico = NotaServico.Excelente,
            NotaPreco = NotaPreco.Justo,
            Comentario = "Tudo certo"
        });

        var avaliacao = await criarAvaliacaoResponse.Content.ReadFromJsonAsync<AvaliacaoResponse>();
        Assert.Equal(HttpStatusCode.OK, criarAvaliacaoResponse.StatusCode);
        Assert.NotNull(avaliacao);

        var moderarResponse = await adminClient.PutAsJsonAsync(
            $"/api/avaliacoes/{avaliacao!.Id}/moderar",
            new ModerarAvaliacaoRequest
            {
                Acao = AcaoModeracaoAvaliacao.Aprovar
            });

        Assert.Equal(HttpStatusCode.OK, moderarResponse.StatusCode);

        notificacoesProfissional = await profissionalClient.GetFromJsonAsync<List<NotificacaoResponse>>("/api/notificacoes/minhas");
        Assert.NotNull(notificacoesProfissional);
        Assert.Contains(notificacoesProfissional!, x => x.Tipo == TipoNotificacao.AvaliacaoAprovada && x.ReferenciaId == avaliacao.Id);

        var quantidadeNaoLidas = await profissionalClient.GetFromJsonAsync<QuantidadeNotificacoesNaoLidasResponse>("/api/notificacoes/minhas/nao-lidas/quantidade");
        Assert.NotNull(quantidadeNaoLidas);
        Assert.True(quantidadeNaoLidas!.Quantidade > 0);

        var notificacao = Assert.Single(notificacoesProfissional.Where(x => x.Tipo == TipoNotificacao.ServicoSolicitado));
        var marcarComoLidaResponse = await profissionalClient.PutAsync($"/api/notificacoes/{notificacao.Id}/marcar-lida", null);
        var notificacaoLida = await marcarComoLidaResponse.Content.ReadFromJsonAsync<NotificacaoResponse>();

        Assert.Equal(HttpStatusCode.OK, marcarComoLidaResponse.StatusCode);
        Assert.NotNull(notificacaoLida);
        Assert.True(notificacaoLida!.Lida);

        var marcarTodasResponse = await profissionalClient.PutAsync("/api/notificacoes/minhas/marcar-todas-lidas", null);
        Assert.Equal(HttpStatusCode.NoContent, marcarTodasResponse.StatusCode);
    }

    [Fact]
    public async Task ConfirmacaoDePagamento_DeveGerarNotificacaoDeImpulsionamentoAtivado()
    {
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional-imp-notif");
        var authAdmin = await LoginAdminAsync(adminClient);

        profissionalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authProfissional.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authAdmin.Token);

        var planos = await profissionalClient.GetFromJsonAsync<List<PlanoImpulsionamentoResponse>>("/api/impulsionamentos/planos");
        Assert.NotNull(planos);
        var plano = Assert.Single(planos!.Where(x => x.QuantidadePeriodo == 1));

        var contratarResponse = await profissionalClient.PostAsJsonAsync("/api/impulsionamentos/contratar", new ContratarPlanoImpulsionamentoRequest
        {
            PlanoImpulsionamentoId = plano.Id,
            CodigoReferenciaPagamento = "notif-imp-001"
        });

        var contratado = await contratarResponse.Content.ReadFromJsonAsync<ImpulsionamentoProfissionalResponse>();
        Assert.Equal(HttpStatusCode.OK, contratarResponse.StatusCode);
        Assert.NotNull(contratado);

        var confirmarResponse = await adminClient.PutAsync($"/api/impulsionamentos/{contratado!.Id}/confirmar-pagamento", null);
        Assert.Equal(HttpStatusCode.OK, confirmarResponse.StatusCode);

        var notificacoes = await profissionalClient.GetFromJsonAsync<List<NotificacaoResponse>>("/api/notificacoes/minhas");
        Assert.NotNull(notificacoes);
        Assert.Contains(notificacoes!, x => x.Tipo == TipoNotificacao.ImpulsionamentoAtivado && x.ReferenciaId == contratado.Id);
    }

    private static async Task<AuthResponse> RegistrarUsuarioAsync(HttpClient client, TipoPerfil tipoPerfil, string prefixo)
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
