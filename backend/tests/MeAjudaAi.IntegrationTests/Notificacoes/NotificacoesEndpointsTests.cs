using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Application.DTOs.Avaliacoes;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.DTOs.Impulsionamentos;
using MeAjudaAi.Application.DTOs.Notificacoes;
using MeAjudaAi.Application.DTOs.Servicos;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using MeAjudaAi.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

    [Fact]
    public async Task ObterResumoOperacionalNotificacoes_DeveConsolidarLidasENaoLidasPorTipoEUsuario()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-resumo-interno");
        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional-resumo-interno");
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
            Titulo = "Servico para resumo interno",
            Descricao = "Gera notificacao interna para o profissional",
            ValorCombinado = 93m
        });

        Assert.Equal(HttpStatusCode.OK, criarServicoResponse.StatusCode);

        var notificacoesProfissional = await profissionalClient.GetFromJsonAsync<List<NotificacaoResponse>>("/api/notificacoes/minhas");
        var notificacao = Assert.Single(notificacoesProfissional!.Where(x => x.Tipo == TipoNotificacao.ServicoSolicitado));

        var marcarLidaResponse = await profissionalClient.PutAsync($"/api/notificacoes/{notificacao.Id}/marcar-lida", null);
        Assert.Equal(HttpStatusCode.OK, marcarLidaResponse.StatusCode);

        var resumo = await adminClient.GetFromJsonAsync<NotificacaoResumoOperacionalResponse>(
            $"/api/notificacoes/resumo-operacional?usuarioId={authProfissional.UsuarioId}&tipoNotificacao={TipoNotificacao.ServicoSolicitado}");

        Assert.NotNull(resumo);
        Assert.Equal(authProfissional.UsuarioId, resumo!.UsuarioId);
        Assert.Equal(TipoNotificacao.ServicoSolicitado, resumo.TipoNotificacao);
        Assert.True(resumo.TotalRegistros >= 1);
        Assert.True(resumo.Lidas >= 1);
        Assert.True(resumo.NaoLidas == 0);
        Assert.Contains(resumo.TopTipos, x =>
            x.TipoNotificacao == TipoNotificacao.ServicoSolicitado &&
            x.Total >= 1 &&
            x.Lidas >= 1);
    }

    [Fact]
    public async Task ListarNotificacoesAdmin_DevePermitirFiltrarPorUsuarioTipoELidaComPaginacao()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-lista-admin-notif");
        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional-lista-admin-notif");
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
            Titulo = "Servico para listagem admin",
            Descricao = "Gera notificacao interna filtravel",
            ValorCombinado = 101m
        });

        Assert.Equal(HttpStatusCode.OK, criarServicoResponse.StatusCode);

        var notificacoesProfissional = await profissionalClient.GetFromJsonAsync<List<NotificacaoResponse>>("/api/notificacoes/minhas");
        var notificacao = Assert.Single(notificacoesProfissional!.Where(x => x.Tipo == TipoNotificacao.ServicoSolicitado));

        var naoLidas = await adminClient.GetFromJsonAsync<PaginacaoResponse<NotificacaoAdminResponse>>(
            $"/api/notificacoes?usuarioId={authProfissional.UsuarioId}&tipoNotificacao={TipoNotificacao.ServicoSolicitado}&lida=false&pagina=1&tamanhoPagina=10");

        Assert.NotNull(naoLidas);
        Assert.Equal(1, naoLidas!.PaginaAtual);
        Assert.Equal(10, naoLidas.TamanhoPagina);
        Assert.True(naoLidas.TotalRegistros >= 1);
        Assert.Contains(naoLidas.Itens, x =>
            x.UsuarioId == authProfissional.UsuarioId &&
            x.Tipo == TipoNotificacao.ServicoSolicitado &&
            !x.Lida &&
            x.EmailUsuario.Contains("teste.local", StringComparison.OrdinalIgnoreCase));

        var marcarLidaResponse = await profissionalClient.PutAsync($"/api/notificacoes/{notificacao.Id}/marcar-lida", null);
        Assert.Equal(HttpStatusCode.OK, marcarLidaResponse.StatusCode);

        var lidas = await adminClient.GetFromJsonAsync<PaginacaoResponse<NotificacaoAdminResponse>>(
            $"/api/notificacoes?usuarioId={authProfissional.UsuarioId}&tipoNotificacao={TipoNotificacao.ServicoSolicitado}&lida=true&pagina=1&tamanhoPagina=10");

        Assert.NotNull(lidas);
        Assert.True(lidas!.TotalRegistros >= 1);
        Assert.Contains(lidas.Itens, x =>
            x.Id == notificacao.Id &&
            x.Lida &&
            x.DataLeitura.HasValue);
    }

    [Fact]
    public async Task ObterNotificacaoAdminPorId_DeveRetornarDetalheOu404()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-detalhe-admin-notif");
        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional-detalhe-admin-notif");
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
            Titulo = "Servico para detalhe admin",
            Descricao = "Gera notificacao para detalhe admin",
            ValorCombinado = 111m
        });

        Assert.Equal(HttpStatusCode.OK, criarServicoResponse.StatusCode);

        var notificacoes = await adminClient.GetFromJsonAsync<PaginacaoResponse<NotificacaoAdminResponse>>(
            $"/api/notificacoes?usuarioId={authProfissional.UsuarioId}&tipoNotificacao={TipoNotificacao.ServicoSolicitado}&pagina=1&tamanhoPagina=10");

        var notificacao = Assert.Single(notificacoes!.Itens);

        var detalheResponse = await adminClient.GetAsync($"/api/notificacoes/{notificacao.Id}");
        var detalhe = await detalheResponse.Content.ReadFromJsonAsync<NotificacaoAdminResponse>();

        Assert.Equal(HttpStatusCode.OK, detalheResponse.StatusCode);
        Assert.NotNull(detalhe);
        Assert.Equal(notificacao.Id, detalhe!.Id);
        Assert.Equal(authProfissional.UsuarioId, detalhe.UsuarioId);
        Assert.Equal(TipoNotificacao.ServicoSolicitado, detalhe.Tipo);

        var inexistenteResponse = await adminClient.GetAsync($"/api/notificacoes/{Guid.NewGuid()}");
        var inexistente = await inexistenteResponse.Content.ReadFromJsonAsync<IntegrationTests.Infrastructure.MensagemErroResponse>();

        Assert.Equal(HttpStatusCode.NotFound, inexistenteResponse.StatusCode);
        Assert.NotNull(inexistente);
        Assert.Equal("Notificação não encontrada.", inexistente!.Mensagem);
    }

    [Fact]
    public async Task MarcarNotificacoesComoLidasEmLote_DevePermitirOperacaoAdminPorFiltro()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-lote-admin-notif");
        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional-lote-admin-notif");
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
            Titulo = "Servico para lote admin",
            Descricao = "Gera notificacao para marcar em lote",
            ValorCombinado = 112m
        });

        Assert.Equal(HttpStatusCode.OK, criarServicoResponse.StatusCode);

        var marcarLoteResponse = await adminClient.PutAsJsonAsync("/api/notificacoes/marcar-lidas-lote", new MarcarNotificacoesComoLidasEmLoteRequest
        {
            UsuarioId = authProfissional.UsuarioId,
            TipoNotificacao = TipoNotificacao.ServicoSolicitado,
            Limite = 10
        });

        var resultado = await marcarLoteResponse.Content.ReadFromJsonAsync<AtualizarEmailsOutboxEmLoteResponse>();

        Assert.Equal(HttpStatusCode.OK, marcarLoteResponse.StatusCode);
        Assert.NotNull(resultado);
        Assert.True(resultado!.QuantidadeAfetada >= 1);

        var lidas = await adminClient.GetFromJsonAsync<PaginacaoResponse<NotificacaoAdminResponse>>(
            $"/api/notificacoes?usuarioId={authProfissional.UsuarioId}&tipoNotificacao={TipoNotificacao.ServicoSolicitado}&lida=true&pagina=1&tamanhoPagina=10");

        Assert.NotNull(lidas);
        Assert.Contains(lidas!.Itens, x =>
            x.UsuarioId == authProfissional.UsuarioId &&
            x.Tipo == TipoNotificacao.ServicoSolicitado &&
            x.Lida &&
            x.DataLeitura.HasValue);
    }

    [Fact]
    public async Task ExportarNotificacoes_DeveRetornarCsvFiltrado()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-exporta-notif");
        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional-exporta-notif");
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
            Titulo = "Servico para exportacao",
            Descricao = "Gera notificacao para exportar",
            ValorCombinado = 120m
        });

        Assert.Equal(HttpStatusCode.OK, criarServicoResponse.StatusCode);

        var exportarResponse = await adminClient.GetAsync(
            $"/api/notificacoes/exportar?usuarioId={authProfissional.UsuarioId}&tipoNotificacao={TipoNotificacao.ServicoSolicitado}&lida=false&limite=100");

        var csv = await exportarResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, exportarResponse.StatusCode);
        Assert.Equal("text/csv; charset=utf-8", exportarResponse.Content.Headers.ContentType!.ToString());
        Assert.Contains("Id,UsuarioId,NomeUsuario,EmailUsuario,Tipo,Titulo,Mensagem,ReferenciaId,Lida,DataCriacao,DataLeitura", csv);
        Assert.Contains("ServicoSolicitado", csv);
        Assert.Contains(authProfissional.UsuarioId.ToString(), csv);
    }

    [Fact]
    public async Task ObterDashboardNotificacoesPorUsuario_DeveConsolidarResumoERecentes()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-dashboard-notif");
        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional-dashboard-notif");
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
            Titulo = "Servico para dashboard",
            Descricao = "Gera notificacao para dashboard interno",
            ValorCombinado = 130m
        });

        Assert.Equal(HttpStatusCode.OK, criarServicoResponse.StatusCode);

        var dashboard = await adminClient.GetFromJsonAsync<NotificacaoUsuarioDashboardResponse>(
            $"/api/notificacoes/usuarios/{authProfissional.UsuarioId}/dashboard?tipoNotificacao={TipoNotificacao.ServicoSolicitado}");

        Assert.NotNull(dashboard);
        Assert.Equal(authProfissional.UsuarioId, dashboard!.UsuarioId);
        Assert.Equal(TipoNotificacao.ServicoSolicitado, dashboard.TipoNotificacao);
        Assert.True(dashboard.Resumo.TotalRegistros >= 1);
        Assert.True(dashboard.Resumo.NaoLidas >= 1);
        Assert.Contains(dashboard.Recentes, x =>
            x.UsuarioId == authProfissional.UsuarioId &&
            x.Tipo == TipoNotificacao.ServicoSolicitado);
    }

    [Fact]
    public async Task ArquivarNotificacoesEmLote_DeveRemoverNotificacoesAtivasDoUsuario()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-arquivar-notif");
        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional-arquivar-notif");
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
            Titulo = "Servico para arquivar",
            Descricao = "Gera notificacao interna para arquivamento",
            ValorCombinado = 140m
        });

        Assert.Equal(HttpStatusCode.OK, criarServicoResponse.StatusCode);

        var minhasAntes = await profissionalClient.GetFromJsonAsync<List<NotificacaoResponse>>("/api/notificacoes/minhas");
        Assert.NotNull(minhasAntes);
        Assert.Contains(minhasAntes!, x => x.Tipo == TipoNotificacao.ServicoSolicitado);

        var arquivarResponse = await adminClient.PutAsJsonAsync("/api/notificacoes/arquivar-lote", new ArquivarNotificacoesEmLoteRequest
        {
            UsuarioId = authProfissional.UsuarioId,
            TipoNotificacao = TipoNotificacao.ServicoSolicitado,
            Lida = false,
            Limite = 100
        });

        var operacao = await arquivarResponse.Content.ReadFromJsonAsync<AtualizarEmailsOutboxEmLoteResponse>();

        Assert.Equal(HttpStatusCode.OK, arquivarResponse.StatusCode);
        Assert.NotNull(operacao);
        Assert.True(operacao!.QuantidadeAfetada >= 1);

        var minhasDepois = await profissionalClient.GetFromJsonAsync<List<NotificacaoResponse>>("/api/notificacoes/minhas");
        Assert.NotNull(minhasDepois);
        Assert.DoesNotContain(minhasDepois!, x => x.Tipo == TipoNotificacao.ServicoSolicitado);
    }

    [Fact]
    public async Task Preferencias_DeveListarDefaultsEPermitirAtualizacao()
    {
        using var client = _factory.CreateClient();

        var auth = await RegistrarUsuarioAsync(client, TipoPerfil.Profissional, "preferencias-notif");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        var preferenciasIniciais = await client.GetFromJsonAsync<List<PreferenciaNotificacaoResponse>>("/api/notificacoes/minhas/preferencias");

        Assert.NotNull(preferenciasIniciais);
        Assert.Equal(Enum.GetValues<TipoNotificacao>().Length, preferenciasIniciais!.Count);
        Assert.All(preferenciasIniciais, x => Assert.True(x.AtivoInterno));

        var atualizarResponse = await client.PutAsJsonAsync("/api/notificacoes/minhas/preferencias", new AtualizarPreferenciasNotificacaoRequest
        {
            Preferencias =
            [
                new PreferenciaNotificacaoItemRequest
                {
                    Tipo = TipoNotificacao.ServicoSolicitado,
                    AtivoInterno = false,
                    AtivoEmail = true
                },
                new PreferenciaNotificacaoItemRequest
                {
                    Tipo = TipoNotificacao.ImpulsionamentoAtivado,
                    AtivoInterno = false,
                    AtivoEmail = false
                }
            ]
        });

        var preferenciasAtualizadas = await atualizarResponse.Content.ReadFromJsonAsync<List<PreferenciaNotificacaoResponse>>();

        Assert.Equal(HttpStatusCode.OK, atualizarResponse.StatusCode);
        Assert.NotNull(preferenciasAtualizadas);
        Assert.Contains(preferenciasAtualizadas!, x => x.Tipo == TipoNotificacao.ServicoSolicitado && !x.AtivoInterno && x.AtivoEmail);
        Assert.Contains(preferenciasAtualizadas!, x => x.Tipo == TipoNotificacao.ImpulsionamentoAtivado && !x.AtivoInterno);
        Assert.Contains(preferenciasAtualizadas!, x => x.Tipo == TipoNotificacao.ServicoAceito && x.AtivoInterno && !x.AtivoEmail);
    }

    [Fact]
    public async Task PreferenciaDesativada_DeveSuprimirNotificacaoInterna()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-pref-sup");
        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional-pref-sup");
        var authAdmin = await LoginAdminAsync(adminClient);

        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authCliente.Token);
        profissionalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authProfissional.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authAdmin.Token);

        var atualizarResponse = await profissionalClient.PutAsJsonAsync("/api/notificacoes/minhas/preferencias", new AtualizarPreferenciasNotificacaoRequest
        {
            Preferencias =
            [
                new PreferenciaNotificacaoItemRequest
                {
                    Tipo = TipoNotificacao.ServicoSolicitado,
                    AtivoInterno = false,
                    AtivoEmail = true
                }
            ]
        });

        Assert.Equal(HttpStatusCode.OK, atualizarResponse.StatusCode);

        var cidadeId = await _factory.ObterCidadeIdAsync();
        var profissionalId = await _factory.ObterProfissionalIdPorUsuarioIdAsync(authProfissional.UsuarioId);

        var criarServicoResponse = await clienteClient.PostAsJsonAsync("/api/servicos", new CriarServicoRequest
        {
            ProfissionalId = profissionalId,
            CidadeId = cidadeId,
            Titulo = "Servico sem notificar",
            Descricao = "Teste de preferencia desativada",
            ValorCombinado = 75m
        });

        Assert.Equal(HttpStatusCode.OK, criarServicoResponse.StatusCode);

        var notificacoesProfissional = await profissionalClient.GetFromJsonAsync<List<NotificacaoResponse>>("/api/notificacoes/minhas");
        var emailsOutbox = await adminClient.GetFromJsonAsync<PaginacaoResponse<EmailNotificacaoOutboxResponse>>($"/api/notificacoes/emails?status={StatusEmailNotificacao.Pendente}&usuarioId={authProfissional.UsuarioId}");

        Assert.NotNull(notificacoesProfissional);
        Assert.DoesNotContain(notificacoesProfissional!, x => x.Tipo == TipoNotificacao.ServicoSolicitado);
        Assert.NotNull(emailsOutbox);
        Assert.Contains(emailsOutbox!.Itens, x =>
            x.TipoNotificacao == TipoNotificacao.ServicoSolicitado &&
            x.UsuarioId == authProfissional.UsuarioId &&
            x.ProximaTentativaEm.HasValue);
    }

    [Fact]
    public async Task AtualizarPreferencias_ComTiposDuplicados_DeveRetornarErroValidacao()
    {
        using var client = _factory.CreateClient();

        var auth = await RegistrarUsuarioAsync(client, TipoPerfil.Profissional, "preferencias-duplicadas");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        var response = await client.PutAsJsonAsync("/api/notificacoes/minhas/preferencias", new AtualizarPreferenciasNotificacaoRequest
        {
            Preferencias =
            [
                new PreferenciaNotificacaoItemRequest
                {
                    Tipo = TipoNotificacao.ServicoSolicitado,
                    AtivoInterno = false,
                    AtivoEmail = false
                },
                new PreferenciaNotificacaoItemRequest
                {
                    Tipo = TipoNotificacao.ServicoSolicitado,
                    AtivoInterno = true,
                    AtivoEmail = true
                }
            ]
        });

        var erro = await response.Content.ReadFromJsonAsync<IntegrationTests.Infrastructure.ErroValidacaoResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(erro);
        Assert.Equal("Erro de validação.", erro!.Mensagem);
        Assert.Contains(erro.Erros, x => x.Campo == "Preferencias");
    }

    [Fact]
    public async Task ListarEmailsOutbox_DeveExigirAdmin()
    {
        using var client = _factory.CreateClient();

        var auth = await RegistrarUsuarioAsync(client, TipoPerfil.Cliente, "emails-outbox-nao-admin");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        var response = await client.GetAsync("/api/notificacoes/emails");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ObterEmailOutboxPorId_DevePermitirConsultaAdminERetornar404QuandoNaoExistir()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-detalhe-email");
        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional-detalhe-email");
        var authAdmin = await LoginAdminAsync(adminClient);

        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authCliente.Token);
        profissionalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authProfissional.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authAdmin.Token);

        var atualizarResponse = await profissionalClient.PutAsJsonAsync("/api/notificacoes/minhas/preferencias", new AtualizarPreferenciasNotificacaoRequest
        {
            Preferencias =
            [
                new PreferenciaNotificacaoItemRequest
                {
                    Tipo = TipoNotificacao.ServicoSolicitado,
                    AtivoInterno = false,
                    AtivoEmail = true
                }
            ]
        });

        Assert.Equal(HttpStatusCode.OK, atualizarResponse.StatusCode);

        var cidadeId = await _factory.ObterCidadeIdAsync();
        var profissionalId = await _factory.ObterProfissionalIdPorUsuarioIdAsync(authProfissional.UsuarioId);

        var criarServicoResponse = await clienteClient.PostAsJsonAsync("/api/servicos", new CriarServicoRequest
        {
            ProfissionalId = profissionalId,
            CidadeId = cidadeId,
            Titulo = "Servico para detalhe",
            Descricao = "Teste do detalhe do outbox",
            ValorCombinado = 95m
        });

        Assert.Equal(HttpStatusCode.OK, criarServicoResponse.StatusCode);

        var emails = await adminClient.GetFromJsonAsync<PaginacaoResponse<EmailNotificacaoOutboxResponse>>(
            $"/api/notificacoes/emails?status={StatusEmailNotificacao.Pendente}&usuarioId={authProfissional.UsuarioId}");
        var email = Assert.Single(emails!.Itens);

        var detalheResponse = await adminClient.GetAsync($"/api/notificacoes/emails/{email.Id}");
        var detalhe = await detalheResponse.Content.ReadFromJsonAsync<EmailNotificacaoOutboxResponse>();

        Assert.Equal(HttpStatusCode.OK, detalheResponse.StatusCode);
        Assert.NotNull(detalhe);
        Assert.Equal(email.Id, detalhe!.Id);
        Assert.Equal(authProfissional.UsuarioId, detalhe.UsuarioId);
        Assert.Equal(TipoNotificacao.ServicoSolicitado, detalhe.TipoNotificacao);

        var inexistenteResponse = await adminClient.GetAsync($"/api/notificacoes/emails/{Guid.NewGuid()}");
        var inexistente = await inexistenteResponse.Content.ReadFromJsonAsync<IntegrationTests.Infrastructure.MensagemErroResponse>();

        Assert.Equal(HttpStatusCode.NotFound, inexistenteResponse.StatusCode);
        Assert.NotNull(inexistente);
        Assert.Equal("E-mail do outbox não encontrado.", inexistente!.Mensagem);
    }

    [Fact]
    public async Task CancelarEReabrirEmailOutbox_DevePermitirControleAdminDaFila()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-cancelar-email");
        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional-cancelar-email");
        var authAdmin = await LoginAdminAsync(adminClient);

        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authCliente.Token);
        profissionalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authProfissional.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authAdmin.Token);

        var atualizarResponse = await profissionalClient.PutAsJsonAsync("/api/notificacoes/minhas/preferencias", new AtualizarPreferenciasNotificacaoRequest
        {
            Preferencias =
            [
                new PreferenciaNotificacaoItemRequest
                {
                    Tipo = TipoNotificacao.ServicoSolicitado,
                    AtivoInterno = false,
                    AtivoEmail = true
                }
            ]
        });

        Assert.Equal(HttpStatusCode.OK, atualizarResponse.StatusCode);

        var cidadeId = await _factory.ObterCidadeIdAsync();
        var profissionalId = await _factory.ObterProfissionalIdPorUsuarioIdAsync(authProfissional.UsuarioId);

        var criarServicoResponse = await clienteClient.PostAsJsonAsync("/api/servicos", new CriarServicoRequest
        {
            ProfissionalId = profissionalId,
            CidadeId = cidadeId,
            Titulo = "Servico para cancelar",
            Descricao = "Teste de cancelamento do outbox",
            ValorCombinado = 88m
        });

        Assert.Equal(HttpStatusCode.OK, criarServicoResponse.StatusCode);

        var emails = await adminClient.GetFromJsonAsync<PaginacaoResponse<EmailNotificacaoOutboxResponse>>(
            $"/api/notificacoes/emails?status={StatusEmailNotificacao.Pendente}&usuarioId={authProfissional.UsuarioId}");
        var email = Assert.Single(emails!.Itens);

        var cancelarResponse = await adminClient.PutAsync($"/api/notificacoes/emails/{email.Id}/cancelar", null);
        var cancelado = await cancelarResponse.Content.ReadFromJsonAsync<EmailNotificacaoOutboxResponse>();

        Assert.Equal(HttpStatusCode.OK, cancelarResponse.StatusCode);
        Assert.NotNull(cancelado);
        Assert.Equal(StatusEmailNotificacao.Cancelado, cancelado!.Status);
        Assert.False(cancelado.ProximaTentativaEm.HasValue);

        var reabrirResponse = await adminClient.PutAsync($"/api/notificacoes/emails/{email.Id}/reabrir", null);
        var reaberto = await reabrirResponse.Content.ReadFromJsonAsync<EmailNotificacaoOutboxResponse>();

        Assert.Equal(HttpStatusCode.OK, reabrirResponse.StatusCode);
        Assert.NotNull(reaberto);
        Assert.Equal(StatusEmailNotificacao.Pendente, reaberto!.Status);
        Assert.True(reaberto.ProximaTentativaEm.HasValue);
    }

    [Fact]
    public async Task ReprocessarEmailsOutbox_DeveMarcarEmailsPendentesComoEnviados()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-reprocessa-email");
        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional-reprocessa-email");
        var authAdmin = await LoginAdminAsync(adminClient);

        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authCliente.Token);
        profissionalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authProfissional.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authAdmin.Token);

        var atualizarResponse = await profissionalClient.PutAsJsonAsync("/api/notificacoes/minhas/preferencias", new AtualizarPreferenciasNotificacaoRequest
        {
            Preferencias =
            [
                new PreferenciaNotificacaoItemRequest
                {
                    Tipo = TipoNotificacao.ServicoSolicitado,
                    AtivoInterno = false,
                    AtivoEmail = true
                }
            ]
        });

        Assert.Equal(HttpStatusCode.OK, atualizarResponse.StatusCode);

        var cidadeId = await _factory.ObterCidadeIdAsync();
        var profissionalId = await _factory.ObterProfissionalIdPorUsuarioIdAsync(authProfissional.UsuarioId);

        var criarServicoResponse = await clienteClient.PostAsJsonAsync("/api/servicos", new CriarServicoRequest
        {
            ProfissionalId = profissionalId,
            CidadeId = cidadeId,
            Titulo = "Servico para outbox",
            Descricao = "Teste de reprocessamento",
            ValorCombinado = 55m
        });

        Assert.Equal(HttpStatusCode.OK, criarServicoResponse.StatusCode);

        var pendentesAntes = await adminClient.GetFromJsonAsync<PaginacaoResponse<EmailNotificacaoOutboxResponse>>($"/api/notificacoes/emails?status={StatusEmailNotificacao.Pendente}&usuarioId={authProfissional.UsuarioId}");
        Assert.NotNull(pendentesAntes);
        Assert.Contains(pendentesAntes!.Itens, x => x.TipoNotificacao == TipoNotificacao.ServicoSolicitado);

        var reprocessarResponse = await adminClient.PostAsync("/api/notificacoes/emails/reprocessar", null);
        var reprocessado = await reprocessarResponse.Content.ReadFromJsonAsync<ReprocessarEmailsOutboxResponse>();

        Assert.Equal(HttpStatusCode.OK, reprocessarResponse.StatusCode);
        Assert.NotNull(reprocessado);
        Assert.True(reprocessado!.QuantidadeProcessada > 0);

        var enviadosDepois = await adminClient.GetFromJsonAsync<PaginacaoResponse<EmailNotificacaoOutboxResponse>>($"/api/notificacoes/emails?status={StatusEmailNotificacao.Enviado}&usuarioId={authProfissional.UsuarioId}");
        Assert.NotNull(enviadosDepois);
        Assert.Contains(enviadosDepois!.Itens, x =>
            x.TipoNotificacao == TipoNotificacao.ServicoSolicitado &&
            x.DataProcessamento.HasValue &&
            x.TentativasProcessamento == 1 &&
            !x.ProximaTentativaEm.HasValue);

        var metricas = await adminClient.GetFromJsonAsync<EmailNotificacaoMetricasResponse>("/api/notificacoes/emails/metricas");
        Assert.NotNull(metricas);
        Assert.Contains(metricas!.Itens, x => x.Status == StatusEmailNotificacao.Enviado && x.Quantidade > 0);
    }

    [Fact]
    public async Task ReprocessarEmailsOutbox_DeveAplicarBackoffECancelarAoExcederTentativas()
    {
        using var configuredFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Emails:Notificacoes:SimularEnvio"] = "false",
                    ["Emails:Notificacoes:MaxTentativas"] = "2",
                    ["Emails:Notificacoes:AtrasoBaseSegundos"] = "30"
                });
            });
        });

        using var clienteClient = configuredFactory.CreateClient();
        using var profissionalClient = configuredFactory.CreateClient();
        using var adminClient = configuredFactory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-retry-email");
        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional-retry-email");
        var authAdmin = await LoginAdminAsync(adminClient);

        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authCliente.Token);
        profissionalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authProfissional.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authAdmin.Token);

        var atualizarResponse = await profissionalClient.PutAsJsonAsync("/api/notificacoes/minhas/preferencias", new AtualizarPreferenciasNotificacaoRequest
        {
            Preferencias =
            [
                new PreferenciaNotificacaoItemRequest
                {
                    Tipo = TipoNotificacao.ServicoSolicitado,
                    AtivoInterno = false,
                    AtivoEmail = true
                }
            ]
        });

        Assert.Equal(HttpStatusCode.OK, atualizarResponse.StatusCode);

        var cidadeId = await _factory.ObterCidadeIdAsync();
        var profissionalId = await _factory.ObterProfissionalIdPorUsuarioIdAsync(authProfissional.UsuarioId);

        var criarServicoResponse = await clienteClient.PostAsJsonAsync("/api/servicos", new CriarServicoRequest
        {
            ProfissionalId = profissionalId,
            CidadeId = cidadeId,
            Titulo = "Servico com retry",
            Descricao = "Teste de backoff do outbox",
            ValorCombinado = 65m
        });

        Assert.Equal(HttpStatusCode.OK, criarServicoResponse.StatusCode);

        var pendentesAntes = await adminClient.GetFromJsonAsync<PaginacaoResponse<EmailNotificacaoOutboxResponse>>($"/api/notificacoes/emails?status={StatusEmailNotificacao.Pendente}&usuarioId={authProfissional.UsuarioId}");
        var emailPendente = Assert.Single(pendentesAntes!.Itens);
        Assert.True(emailPendente.ProximaTentativaEm.HasValue);

        var primeiraTentativaResponse = await adminClient.PostAsync("/api/notificacoes/emails/reprocessar", null);
        var primeiraTentativa = await primeiraTentativaResponse.Content.ReadFromJsonAsync<ReprocessarEmailsOutboxResponse>();

        Assert.Equal(HttpStatusCode.OK, primeiraTentativaResponse.StatusCode);
        Assert.NotNull(primeiraTentativa);
        Assert.Equal(1, primeiraTentativa!.QuantidadeProcessada);

        var falhasDepoisDaPrimeiraTentativa = await adminClient.GetFromJsonAsync<PaginacaoResponse<EmailNotificacaoOutboxResponse>>($"/api/notificacoes/emails?status={StatusEmailNotificacao.Falha}&usuarioId={authProfissional.UsuarioId}");
        var emailComFalha = Assert.Single(falhasDepoisDaPrimeiraTentativa!.Itens);

        Assert.Equal(1, emailComFalha.TentativasProcessamento);
        Assert.True(emailComFalha.ProximaTentativaEm.HasValue);
        Assert.NotNull(emailComFalha.DataProcessamento);
        Assert.NotEmpty(emailComFalha.UltimaMensagemErro);
        Assert.True(emailComFalha.ProximaTentativaEm > emailComFalha.DataProcessamento);

        await using (var scope = configuredFactory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var email = await context.EmailsNotificacoesOutbox.SingleAsync(x => x.Id == emailComFalha.Id);
            email.ProximaTentativaEm = DateTime.UtcNow.AddMinutes(-1);
            await context.SaveChangesAsync();
        }

        var segundaTentativaResponse = await adminClient.PostAsync("/api/notificacoes/emails/reprocessar", null);
        var segundaTentativa = await segundaTentativaResponse.Content.ReadFromJsonAsync<ReprocessarEmailsOutboxResponse>();

        Assert.Equal(HttpStatusCode.OK, segundaTentativaResponse.StatusCode);
        Assert.NotNull(segundaTentativa);
        Assert.Equal(1, segundaTentativa!.QuantidadeProcessada);

        var cancelados = await adminClient.GetFromJsonAsync<PaginacaoResponse<EmailNotificacaoOutboxResponse>>($"/api/notificacoes/emails?status={StatusEmailNotificacao.Cancelado}&usuarioId={authProfissional.UsuarioId}");
        var emailCancelado = Assert.Single(cancelados!.Itens);

        Assert.Equal(2, emailCancelado.TentativasProcessamento);
        Assert.False(emailCancelado.ProximaTentativaEm.HasValue);
        Assert.NotNull(emailCancelado.DataProcessamento);
        Assert.NotEmpty(emailCancelado.UltimaMensagemErro);

        var metricas = await adminClient.GetFromJsonAsync<EmailNotificacaoMetricasResponse>("/api/notificacoes/emails/metricas");
        Assert.NotNull(metricas);
        Assert.Contains(metricas!.Itens, x => x.Status == StatusEmailNotificacao.Cancelado && x.Quantidade > 0);
    }

    [Fact]
    public async Task PreviewEmail_DeveExigirAdminERenderizarHtml()
    {
        using var client = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(client, TipoPerfil.Cliente, "preview-email-nao-admin");
        var authAdmin = await LoginAdminAsync(adminClient);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authCliente.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authAdmin.Token);

        var forbiddenResponse = await client.PostAsJsonAsync("/api/notificacoes/emails/preview", new PreviewEmailNotificacaoRequest
        {
            TipoNotificacao = TipoNotificacao.ServicoSolicitado,
            Assunto = "Servico solicitado",
            Corpo = "Um novo servico foi criado para voce."
        });

        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);

        var referenciaId = Guid.NewGuid();
        var previewResponse = await adminClient.PostAsJsonAsync("/api/notificacoes/emails/preview", new PreviewEmailNotificacaoRequest
        {
            TipoNotificacao = TipoNotificacao.ServicoSolicitado,
            Assunto = "Servico solicitado",
            Corpo = "Um novo servico foi criado para voce.",
            ReferenciaId = referenciaId
        });

        var preview = await previewResponse.Content.ReadFromJsonAsync<PreviewEmailNotificacaoResponse>();

        Assert.Equal(HttpStatusCode.OK, previewResponse.StatusCode);
        Assert.NotNull(preview);
        Assert.Equal(TipoNotificacao.ServicoSolicitado, preview!.TipoNotificacao);
        Assert.Equal("Servico solicitado", preview.Assunto);
        Assert.Equal(referenciaId, preview.ReferenciaId);
        Assert.Contains("Um novo servico foi criado para voce.", preview.Html);
        Assert.Contains("Referencia:", preview.Html);
    }

    [Fact]
    public async Task ListarEmailsOutbox_DevePermitirFiltrarPorTipoDestinoEDataCriacao()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-filtro-email");
        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional-filtro-email");
        var authAdmin = await LoginAdminAsync(adminClient);

        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authCliente.Token);
        profissionalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authProfissional.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authAdmin.Token);

        var atualizarResponse = await profissionalClient.PutAsJsonAsync("/api/notificacoes/minhas/preferencias", new AtualizarPreferenciasNotificacaoRequest
        {
            Preferencias =
            [
                new PreferenciaNotificacaoItemRequest
                {
                    Tipo = TipoNotificacao.ServicoSolicitado,
                    AtivoInterno = false,
                    AtivoEmail = true
                }
            ]
        });

        Assert.Equal(HttpStatusCode.OK, atualizarResponse.StatusCode);

        var cidadeId = await _factory.ObterCidadeIdAsync();
        var profissionalId = await _factory.ObterProfissionalIdPorUsuarioIdAsync(authProfissional.UsuarioId);

        var criarServicoResponse = await clienteClient.PostAsJsonAsync("/api/servicos", new CriarServicoRequest
        {
            ProfissionalId = profissionalId,
            CidadeId = cidadeId,
            Titulo = "Servico para filtro",
            Descricao = "Teste de filtro do outbox",
            ValorCombinado = 80m
        });

        Assert.Equal(HttpStatusCode.OK, criarServicoResponse.StatusCode);

        var inicioJanela = DateTime.UtcNow.AddMinutes(-1).ToString("O");
        var fimJanela = DateTime.UtcNow.AddMinutes(1).ToString("O");
        var emailParcial = "teste.local";

        var filtrados = await adminClient.GetFromJsonAsync<PaginacaoResponse<EmailNotificacaoOutboxResponse>>(
            $"/api/notificacoes/emails?status={StatusEmailNotificacao.Pendente}&usuarioId={authProfissional.UsuarioId}&tipoNotificacao={TipoNotificacao.ServicoSolicitado}&emailDestino={emailParcial}&dataCriacaoInicial={Uri.EscapeDataString(inicioJanela)}&dataCriacaoFinal={Uri.EscapeDataString(fimJanela)}&pagina=1&tamanhoPagina=10");

        var foraDaJanela = await adminClient.GetFromJsonAsync<PaginacaoResponse<EmailNotificacaoOutboxResponse>>(
            $"/api/notificacoes/emails?usuarioId={authProfissional.UsuarioId}&dataCriacaoInicial={Uri.EscapeDataString(DateTime.UtcNow.AddDays(1).ToString("O"))}");

        Assert.NotNull(filtrados);
        Assert.Equal(1, filtrados!.PaginaAtual);
        Assert.Equal(10, filtrados.TamanhoPagina);
        Assert.Contains(filtrados!.Itens, x =>
            x.UsuarioId == authProfissional.UsuarioId &&
            x.TipoNotificacao == TipoNotificacao.ServicoSolicitado &&
            x.EmailDestino.Contains(emailParcial, StringComparison.OrdinalIgnoreCase));
        Assert.True(filtrados.TotalRegistros > 0);

        Assert.NotNull(foraDaJanela);
        Assert.Empty(foraDaJanela!.Itens);
    }

    [Fact]
    public async Task ListarEmailsOutbox_DevePermitirOrdenacaoConfiguravel()
    {
        using var adminClient = _factory.CreateClient();
        var authAdmin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authAdmin.Token);

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var usuarioId = Guid.NewGuid();

            context.Usuarios.Add(new Domain.Entities.Usuario
            {
                Id = usuarioId,
                Nome = "usuario ordenacao outbox",
                Email = $"ordenacao-outbox-{Guid.NewGuid():N}@teste.local",
                Telefone = "11999999999",
                SenhaHash = "hash",
                TipoPerfil = TipoPerfil.Cliente
            });

            context.EmailsNotificacoesOutbox.AddRange(
                new Domain.Entities.EmailNotificacaoOutbox
                {
                    UsuarioId = usuarioId,
                    TipoNotificacao = TipoNotificacao.ServicoSolicitado,
                    EmailDestino = "zzz@teste.local",
                    Assunto = "z",
                    Corpo = "z",
                    Status = StatusEmailNotificacao.Pendente,
                    ProximaTentativaEm = DateTime.UtcNow
                },
                new Domain.Entities.EmailNotificacaoOutbox
                {
                    UsuarioId = usuarioId,
                    TipoNotificacao = TipoNotificacao.ServicoSolicitado,
                    EmailDestino = "aaa@teste.local",
                    Assunto = "a",
                    Corpo = "a",
                    Status = StatusEmailNotificacao.Pendente,
                    ProximaTentativaEm = DateTime.UtcNow
                });

            await context.SaveChangesAsync();
        }

        var response = await adminClient.GetFromJsonAsync<PaginacaoResponse<EmailNotificacaoOutboxResponse>>(
            "/api/notificacoes/emails?ordenarPor=emailDestino&ordemDesc=false&pagina=1&tamanhoPagina=20");

        Assert.NotNull(response);
        Assert.True(response!.Itens.Count >= 2);
        Assert.True(string.Compare(response.Itens[0].EmailDestino, response.Itens[1].EmailDestino, StringComparison.OrdinalIgnoreCase) <= 0);
    }

    [Fact]
    public async Task ObterMetricasEmailsOutbox_DevePermitirFiltrarPorTipoDestinoEPeriodo()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-metrica-email");
        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional-metrica-email");
        var authAdmin = await LoginAdminAsync(adminClient);

        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authCliente.Token);
        profissionalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authProfissional.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authAdmin.Token);

        var atualizarResponse = await profissionalClient.PutAsJsonAsync("/api/notificacoes/minhas/preferencias", new AtualizarPreferenciasNotificacaoRequest
        {
            Preferencias =
            [
                new PreferenciaNotificacaoItemRequest
                {
                    Tipo = TipoNotificacao.ServicoSolicitado,
                    AtivoInterno = false,
                    AtivoEmail = true
                }
            ]
        });

        Assert.Equal(HttpStatusCode.OK, atualizarResponse.StatusCode);

        var cidadeId = await _factory.ObterCidadeIdAsync();
        var profissionalId = await _factory.ObterProfissionalIdPorUsuarioIdAsync(authProfissional.UsuarioId);

        var criarServicoResponse = await clienteClient.PostAsJsonAsync("/api/servicos", new CriarServicoRequest
        {
            ProfissionalId = profissionalId,
            CidadeId = cidadeId,
            Titulo = "Servico para metrica",
            Descricao = "Teste de metricas filtradas",
            ValorCombinado = 77m
        });

        Assert.Equal(HttpStatusCode.OK, criarServicoResponse.StatusCode);

        var inicioJanela = Uri.EscapeDataString(DateTime.UtcNow.AddMinutes(-1).ToString("O"));
        var fimJanela = Uri.EscapeDataString(DateTime.UtcNow.AddMinutes(1).ToString("O"));

        var metricas = await adminClient.GetFromJsonAsync<EmailNotificacaoMetricasResponse>(
            $"/api/notificacoes/emails/metricas?tipoNotificacao={TipoNotificacao.ServicoSolicitado}&emailDestino=teste.local&dataCriacaoInicial={inicioJanela}&dataCriacaoFinal={fimJanela}");

        Assert.NotNull(metricas);
        Assert.Equal(TipoNotificacao.ServicoSolicitado, metricas!.TipoNotificacao);
        Assert.Equal("teste.local", metricas.EmailDestino);
        Assert.True(metricas.TotalRegistros > 0);
        Assert.Contains(metricas.Itens, x => x.Status == StatusEmailNotificacao.Pendente && x.Quantidade > 0);
    }

    [Fact]
    public async Task ObterResumoOperacionalEmailsOutbox_DeveConsolidarFilaProntaEAguardandoRetry()
    {
        using var configuredFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Emails:Notificacoes:SimularEnvio"] = "false",
                    ["Emails:Notificacoes:MaxTentativas"] = "2",
                    ["Emails:Notificacoes:AtrasoBaseSegundos"] = "30"
                });
            });
        });

        using var clienteClient = configuredFactory.CreateClient();
        using var profissionalClient = configuredFactory.CreateClient();
        using var adminClient = configuredFactory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-resumo-operacional");
        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional-resumo-operacional");
        var authAdmin = await LoginAdminAsync(adminClient);

        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authCliente.Token);
        profissionalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authProfissional.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authAdmin.Token);

        var atualizarResponse = await profissionalClient.PutAsJsonAsync("/api/notificacoes/minhas/preferencias", new AtualizarPreferenciasNotificacaoRequest
        {
            Preferencias =
            [
                new PreferenciaNotificacaoItemRequest
                {
                    Tipo = TipoNotificacao.ServicoSolicitado,
                    AtivoInterno = false,
                    AtivoEmail = true
                }
            ]
        });

        Assert.Equal(HttpStatusCode.OK, atualizarResponse.StatusCode);

        Guid cidadeId;
        Guid profissionalId;

        await using (var scope = configuredFactory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            cidadeId = await context.Cidades.Select(x => x.Id).FirstAsync();
            profissionalId = await context.Profissionais
                .Where(x => x.UsuarioId == authProfissional.UsuarioId)
                .Select(x => x.Id)
                .FirstAsync();
        }

        var primeiroServicoResponse = await clienteClient.PostAsJsonAsync("/api/servicos", new CriarServicoRequest
        {
            ProfissionalId = profissionalId,
            CidadeId = cidadeId,
            Titulo = "Servico para falha",
            Descricao = "Primeiro email vai falhar",
            ValorCombinado = 71m
        });

        Assert.Equal(HttpStatusCode.OK, primeiroServicoResponse.StatusCode);

        var reprocessarResponse = await adminClient.PostAsync("/api/notificacoes/emails/reprocessar", null);
        var reprocessado = await reprocessarResponse.Content.ReadFromJsonAsync<ReprocessarEmailsOutboxResponse>();

        Assert.Equal(HttpStatusCode.OK, reprocessarResponse.StatusCode);
        Assert.NotNull(reprocessado);
        Assert.Equal(1, reprocessado!.QuantidadeProcessada);

        var segundoServicoResponse = await clienteClient.PostAsJsonAsync("/api/servicos", new CriarServicoRequest
        {
            ProfissionalId = profissionalId,
            CidadeId = cidadeId,
            Titulo = "Servico pendente",
            Descricao = "Segundo email fica pronto para processar",
            ValorCombinado = 72m
        });

        Assert.Equal(HttpStatusCode.OK, segundoServicoResponse.StatusCode);

        var resumo = await adminClient.GetFromJsonAsync<EmailNotificacaoResumoOperacionalResponse>(
            $"/api/notificacoes/emails/resumo-operacional?usuarioId={authProfissional.UsuarioId}&tipoNotificacao={TipoNotificacao.ServicoSolicitado}");

        Assert.NotNull(resumo);
        Assert.Equal(authProfissional.UsuarioId, resumo!.UsuarioId);
        Assert.Equal(TipoNotificacao.ServicoSolicitado, resumo.TipoNotificacao);
        Assert.True(resumo.TotalRegistros >= 2);
        Assert.True(resumo.Pendentes >= 1);
        Assert.True(resumo.Falhas >= 1);
        Assert.True(resumo.ProntosParaProcessar >= 1);
        Assert.True(resumo.AguardandoProximaTentativa >= 1);
        Assert.Contains(resumo.TopTiposComFalha, x => x.TipoNotificacao == TipoNotificacao.ServicoSolicitado && x.QuantidadeFalhas > 0);
        Assert.Contains(resumo.TopDestinatariosComFalha, x =>
            x.UsuarioId == authProfissional.UsuarioId &&
            x.EmailDestino.Contains("teste.local", StringComparison.OrdinalIgnoreCase) &&
            x.QuantidadeFalhas > 0);
    }

    [Fact]
    public async Task ObterMetricasSerieEmailsOutbox_DeveAgruparPorDiaTipoEStatus()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-serie-email");
        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional-serie-email");
        var authAdmin = await LoginAdminAsync(adminClient);

        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authCliente.Token);
        profissionalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authProfissional.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authAdmin.Token);

        var atualizarResponse = await profissionalClient.PutAsJsonAsync("/api/notificacoes/minhas/preferencias", new AtualizarPreferenciasNotificacaoRequest
        {
            Preferencias =
            [
                new PreferenciaNotificacaoItemRequest
                {
                    Tipo = TipoNotificacao.ServicoSolicitado,
                    AtivoInterno = false,
                    AtivoEmail = true
                }
            ]
        });

        Assert.Equal(HttpStatusCode.OK, atualizarResponse.StatusCode);

        var cidadeId = await _factory.ObterCidadeIdAsync();
        var profissionalId = await _factory.ObterProfissionalIdPorUsuarioIdAsync(authProfissional.UsuarioId);

        var criarServicoResponse = await clienteClient.PostAsJsonAsync("/api/servicos", new CriarServicoRequest
        {
            ProfissionalId = profissionalId,
            CidadeId = cidadeId,
            Titulo = "Servico para serie",
            Descricao = "Teste de serie temporal",
            ValorCombinado = 66m
        });

        Assert.Equal(HttpStatusCode.OK, criarServicoResponse.StatusCode);

        var inicioJanela = Uri.EscapeDataString(DateTime.UtcNow.AddMinutes(-1).ToString("O"));
        var fimJanela = Uri.EscapeDataString(DateTime.UtcNow.AddMinutes(1).ToString("O"));

        var serie = await adminClient.GetFromJsonAsync<EmailNotificacaoMetricasSerieResponse>(
            $"/api/notificacoes/emails/metricas/serie?tipoNotificacao={TipoNotificacao.ServicoSolicitado}&emailDestino=teste.local&dataCriacaoInicial={inicioJanela}&dataCriacaoFinal={fimJanela}");

        Assert.NotNull(serie);
        Assert.Equal(TipoNotificacao.ServicoSolicitado, serie!.TipoNotificacao);
        Assert.Equal("teste.local", serie.EmailDestino);
        Assert.True(serie.TotalRegistros > 0);
        Assert.Contains(serie.Itens, x =>
            x.TipoNotificacao == TipoNotificacao.ServicoSolicitado &&
            x.Status == StatusEmailNotificacao.Pendente &&
            x.Quantidade > 0 &&
            x.Data.Date == DateTime.UtcNow.Date);
    }

    [Fact]
    public async Task ObterMetricasDestinatariosEmailsOutbox_DeveAgruparPorDestinatarioEStatus()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-dest-email");
        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional-dest-email");
        var authAdmin = await LoginAdminAsync(adminClient);

        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authCliente.Token);
        profissionalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authProfissional.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authAdmin.Token);

        var atualizarResponse = await profissionalClient.PutAsJsonAsync("/api/notificacoes/minhas/preferencias", new AtualizarPreferenciasNotificacaoRequest
        {
            Preferencias =
            [
                new PreferenciaNotificacaoItemRequest
                {
                    Tipo = TipoNotificacao.ServicoSolicitado,
                    AtivoInterno = false,
                    AtivoEmail = true
                }
            ]
        });

        Assert.Equal(HttpStatusCode.OK, atualizarResponse.StatusCode);

        var cidadeId = await _factory.ObterCidadeIdAsync();
        var profissionalId = await _factory.ObterProfissionalIdPorUsuarioIdAsync(authProfissional.UsuarioId);

        var criarServicoResponse = await clienteClient.PostAsJsonAsync("/api/servicos", new CriarServicoRequest
        {
            ProfissionalId = profissionalId,
            CidadeId = cidadeId,
            Titulo = "Servico para destinatario",
            Descricao = "Teste de metricas por destinatario",
            ValorCombinado = 88m
        });

        Assert.Equal(HttpStatusCode.OK, criarServicoResponse.StatusCode);

        var inicioJanela = Uri.EscapeDataString(DateTime.UtcNow.AddMinutes(-1).ToString("O"));
        var fimJanela = Uri.EscapeDataString(DateTime.UtcNow.AddMinutes(1).ToString("O"));

        var metricas = await adminClient.GetFromJsonAsync<EmailNotificacaoDestinatariosMetricasResponse>(
            $"/api/notificacoes/emails/metricas/destinatarios?tipoNotificacao={TipoNotificacao.ServicoSolicitado}&emailDestino=teste.local&dataCriacaoInicial={inicioJanela}&dataCriacaoFinal={fimJanela}");

        Assert.NotNull(metricas);
        Assert.Equal(TipoNotificacao.ServicoSolicitado, metricas!.TipoNotificacao);
        Assert.Equal("teste.local", metricas.EmailDestino);
        Assert.True(metricas.TotalRegistros > 0);
        Assert.True(metricas.TotalDestinatarios > 0);
        Assert.Contains(metricas.Itens, x =>
            x.UsuarioId == authProfissional.UsuarioId &&
            x.EmailDestino.Contains("teste.local", StringComparison.OrdinalIgnoreCase) &&
            x.Total > 0 &&
            x.Pendentes > 0);
    }

    [Fact]
    public async Task ObterMetricasTiposEmailsOutbox_DeveAgruparPorTipoEStatus()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-tipo-email");
        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional-tipo-email");
        var authAdmin = await LoginAdminAsync(adminClient);

        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authCliente.Token);
        profissionalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authProfissional.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authAdmin.Token);

        var atualizarResponse = await profissionalClient.PutAsJsonAsync("/api/notificacoes/minhas/preferencias", new AtualizarPreferenciasNotificacaoRequest
        {
            Preferencias =
            [
                new PreferenciaNotificacaoItemRequest
                {
                    Tipo = TipoNotificacao.ServicoSolicitado,
                    AtivoInterno = false,
                    AtivoEmail = true
                }
            ]
        });

        Assert.Equal(HttpStatusCode.OK, atualizarResponse.StatusCode);

        var cidadeId = await _factory.ObterCidadeIdAsync();
        var profissionalId = await _factory.ObterProfissionalIdPorUsuarioIdAsync(authProfissional.UsuarioId);

        var criarServicoResponse = await clienteClient.PostAsJsonAsync("/api/servicos", new CriarServicoRequest
        {
            ProfissionalId = profissionalId,
            CidadeId = cidadeId,
            Titulo = "Servico para tipo",
            Descricao = "Teste de metricas por tipo",
            ValorCombinado = 99m
        });

        Assert.Equal(HttpStatusCode.OK, criarServicoResponse.StatusCode);

        var inicioJanela = Uri.EscapeDataString(DateTime.UtcNow.AddMinutes(-1).ToString("O"));
        var fimJanela = Uri.EscapeDataString(DateTime.UtcNow.AddMinutes(1).ToString("O"));

        var metricas = await adminClient.GetFromJsonAsync<EmailNotificacaoTiposMetricasResponse>(
            $"/api/notificacoes/emails/metricas/tipos?emailDestino=teste.local&dataCriacaoInicial={inicioJanela}&dataCriacaoFinal={fimJanela}");

        Assert.NotNull(metricas);
        Assert.Equal("teste.local", metricas!.EmailDestino);
        Assert.True(metricas.TotalRegistros > 0);
        Assert.Contains(metricas.Itens, x =>
            x.TipoNotificacao == TipoNotificacao.ServicoSolicitado &&
            x.Total > 0 &&
            x.Pendentes > 0);
    }

    [Fact]
    public async Task ObterDashboardEmailsOutbox_DeveConsolidarAsMetricasEmUmaResposta()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-dashboard-email");
        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional-dashboard-email");
        var authAdmin = await LoginAdminAsync(adminClient);

        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authCliente.Token);
        profissionalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authProfissional.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authAdmin.Token);

        var atualizarResponse = await profissionalClient.PutAsJsonAsync("/api/notificacoes/minhas/preferencias", new AtualizarPreferenciasNotificacaoRequest
        {
            Preferencias =
            [
                new PreferenciaNotificacaoItemRequest
                {
                    Tipo = TipoNotificacao.ServicoSolicitado,
                    AtivoInterno = false,
                    AtivoEmail = true
                }
            ]
        });

        Assert.Equal(HttpStatusCode.OK, atualizarResponse.StatusCode);

        var cidadeId = await _factory.ObterCidadeIdAsync();
        var profissionalId = await _factory.ObterProfissionalIdPorUsuarioIdAsync(authProfissional.UsuarioId);

        var criarServicoResponse = await clienteClient.PostAsJsonAsync("/api/servicos", new CriarServicoRequest
        {
            ProfissionalId = profissionalId,
            CidadeId = cidadeId,
            Titulo = "Servico para dashboard",
            Descricao = "Teste de dashboard de metricas",
            ValorCombinado = 111m
        });

        Assert.Equal(HttpStatusCode.OK, criarServicoResponse.StatusCode);

        var inicioJanela = Uri.EscapeDataString(DateTime.UtcNow.AddMinutes(-1).ToString("O"));
        var fimJanela = Uri.EscapeDataString(DateTime.UtcNow.AddMinutes(1).ToString("O"));

        var dashboard = await adminClient.GetFromJsonAsync<EmailNotificacaoDashboardResponse>(
            $"/api/notificacoes/emails/dashboard?tipoNotificacao={TipoNotificacao.ServicoSolicitado}&emailDestino=teste.local&dataCriacaoInicial={inicioJanela}&dataCriacaoFinal={fimJanela}");

        Assert.NotNull(dashboard);
        Assert.Equal(TipoNotificacao.ServicoSolicitado, dashboard!.TipoNotificacao);
        Assert.Equal("teste.local", dashboard.EmailDestino);
        Assert.True(dashboard.Resumo.TotalRegistros > 0);
        Assert.NotEmpty(dashboard.Resumo.Itens);
        Assert.NotEmpty(dashboard.Serie.Itens);
        Assert.NotEmpty(dashboard.Destinatarios.Itens);
        Assert.NotEmpty(dashboard.Tipos.Itens);
    }

    [Fact]
    public async Task AtualizarEmailsOutboxEmLote_DeveCancelarEReabrirPorFiltro()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-lote-email");
        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional-lote-email");
        var authAdmin = await LoginAdminAsync(adminClient);

        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authCliente.Token);
        profissionalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authProfissional.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authAdmin.Token);

        var atualizarResponse = await profissionalClient.PutAsJsonAsync("/api/notificacoes/minhas/preferencias", new AtualizarPreferenciasNotificacaoRequest
        {
            Preferencias =
            [
                new PreferenciaNotificacaoItemRequest
                {
                    Tipo = TipoNotificacao.ServicoSolicitado,
                    AtivoInterno = false,
                    AtivoEmail = true
                }
            ]
        });

        Assert.Equal(HttpStatusCode.OK, atualizarResponse.StatusCode);

        var cidadeId = await _factory.ObterCidadeIdAsync();
        var profissionalId = await _factory.ObterProfissionalIdPorUsuarioIdAsync(authProfissional.UsuarioId);

        var criarServicoResponse = await clienteClient.PostAsJsonAsync("/api/servicos", new CriarServicoRequest
        {
            ProfissionalId = profissionalId,
            CidadeId = cidadeId,
            Titulo = "Servico para lote",
            Descricao = "Teste de operacao em lote",
            ValorCombinado = 123m
        });

        Assert.Equal(HttpStatusCode.OK, criarServicoResponse.StatusCode);

        var cancelarResponse = await adminClient.PutAsJsonAsync("/api/notificacoes/emails/cancelar-lote", new AtualizarEmailsOutboxEmLoteRequest
        {
            Status = StatusEmailNotificacao.Pendente,
            UsuarioId = authProfissional.UsuarioId,
            TipoNotificacao = TipoNotificacao.ServicoSolicitado,
            Limite = 10
        });

        var cancelamento = await cancelarResponse.Content.ReadFromJsonAsync<AtualizarEmailsOutboxEmLoteResponse>();

        Assert.Equal(HttpStatusCode.OK, cancelarResponse.StatusCode);
        Assert.NotNull(cancelamento);
        Assert.True(cancelamento!.QuantidadeAfetada > 0);

        var cancelados = await adminClient.GetFromJsonAsync<PaginacaoResponse<EmailNotificacaoOutboxResponse>>(
            $"/api/notificacoes/emails?status={StatusEmailNotificacao.Cancelado}&usuarioId={authProfissional.UsuarioId}&tipoNotificacao={TipoNotificacao.ServicoSolicitado}");
        Assert.NotNull(cancelados);
        Assert.NotEmpty(cancelados!.Itens);

        var reabrirResponse = await adminClient.PutAsJsonAsync("/api/notificacoes/emails/reabrir-lote", new AtualizarEmailsOutboxEmLoteRequest
        {
            Status = StatusEmailNotificacao.Cancelado,
            UsuarioId = authProfissional.UsuarioId,
            TipoNotificacao = TipoNotificacao.ServicoSolicitado,
            Limite = 10
        });

        var reabertura = await reabrirResponse.Content.ReadFromJsonAsync<AtualizarEmailsOutboxEmLoteResponse>();

        Assert.Equal(HttpStatusCode.OK, reabrirResponse.StatusCode);
        Assert.NotNull(reabertura);
        Assert.True(reabertura!.QuantidadeAfetada > 0);

        var pendentes = await adminClient.GetFromJsonAsync<PaginacaoResponse<EmailNotificacaoOutboxResponse>>(
            $"/api/notificacoes/emails?status={StatusEmailNotificacao.Pendente}&usuarioId={authProfissional.UsuarioId}&tipoNotificacao={TipoNotificacao.ServicoSolicitado}");
        Assert.NotNull(pendentes);
        Assert.NotEmpty(pendentes!.Itens);
    }

    [Fact]
    public async Task AtualizarEmailsOutboxEmLote_SemFiltro_DeveRetornarErroValidacao()
    {
        using var adminClient = _factory.CreateClient();
        var authAdmin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authAdmin.Token);

        var response = await adminClient.PutAsJsonAsync("/api/notificacoes/emails/cancelar-lote", new AtualizarEmailsOutboxEmLoteRequest
        {
            Limite = 10
        });

        var erro = await response.Content.ReadFromJsonAsync<IntegrationTests.Infrastructure.ErroValidacaoResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(erro);
        Assert.Contains(erro!.Erros, x => x.Campo == string.Empty || x.Campo == "request");
    }

    [Fact]
    public async Task ExportarEmailsOutbox_DeveRetornarCsvFiltrado()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-export-email");
        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional-export-email");
        var authAdmin = await LoginAdminAsync(adminClient);

        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authCliente.Token);
        profissionalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authProfissional.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authAdmin.Token);

        var atualizarResponse = await profissionalClient.PutAsJsonAsync("/api/notificacoes/minhas/preferencias", new AtualizarPreferenciasNotificacaoRequest
        {
            Preferencias =
            [
                new PreferenciaNotificacaoItemRequest
                {
                    Tipo = TipoNotificacao.ServicoSolicitado,
                    AtivoInterno = false,
                    AtivoEmail = true
                }
            ]
        });

        Assert.Equal(HttpStatusCode.OK, atualizarResponse.StatusCode);

        var cidadeId = await _factory.ObterCidadeIdAsync();
        var profissionalId = await _factory.ObterProfissionalIdPorUsuarioIdAsync(authProfissional.UsuarioId);

        var criarServicoResponse = await clienteClient.PostAsJsonAsync("/api/servicos", new CriarServicoRequest
        {
            ProfissionalId = profissionalId,
            CidadeId = cidadeId,
            Titulo = "Servico para exportacao",
            Descricao = "Teste de exportacao csv",
            ValorCombinado = 130m
        });

        Assert.Equal(HttpStatusCode.OK, criarServicoResponse.StatusCode);

        var response = await adminClient.GetAsync(
            $"/api/notificacoes/emails/exportar?status={StatusEmailNotificacao.Pendente}&usuarioId={authProfissional.UsuarioId}&tipoNotificacao={TipoNotificacao.ServicoSolicitado}&limite=10");

        var csv = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv; charset=utf-8", response.Content.Headers.ContentType!.ToString());
        Assert.Contains("Id,UsuarioId,TipoNotificacao,EmailDestino,Assunto,Status", csv);
        Assert.Contains(authProfissional.UsuarioId.ToString(), csv);
        Assert.Contains("ServicoSolicitado", csv);
        Assert.Contains("Pendente", csv);
    }

    [Fact]
    public async Task ExportarEmailsOutbox_SemFiltro_DeveRetornarErroValidacao()
    {
        using var adminClient = _factory.CreateClient();
        var authAdmin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authAdmin.Token);

        var response = await adminClient.GetAsync("/api/notificacoes/emails/exportar?limite=10");
        var erro = await response.Content.ReadFromJsonAsync<IntegrationTests.Infrastructure.ErroValidacaoResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(erro);
        Assert.Contains(erro!.Erros, x => x.Campo == string.Empty || x.Campo == "request");
    }

    [Fact]
    public async Task ObterDashboardEmailsOutboxPorUsuario_DeveRetornarDrillDownDoDestinatario()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-dashboard-user-email");
        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional-dashboard-user-email");
        var authAdmin = await LoginAdminAsync(adminClient);

        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authCliente.Token);
        profissionalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authProfissional.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authAdmin.Token);

        var atualizarResponse = await profissionalClient.PutAsJsonAsync("/api/notificacoes/minhas/preferencias", new AtualizarPreferenciasNotificacaoRequest
        {
            Preferencias =
            [
                new PreferenciaNotificacaoItemRequest
                {
                    Tipo = TipoNotificacao.ServicoSolicitado,
                    AtivoInterno = false,
                    AtivoEmail = true
                }
            ]
        });

        Assert.Equal(HttpStatusCode.OK, atualizarResponse.StatusCode);

        var cidadeId = await _factory.ObterCidadeIdAsync();
        var profissionalId = await _factory.ObterProfissionalIdPorUsuarioIdAsync(authProfissional.UsuarioId);

        var criarServicoResponse = await clienteClient.PostAsJsonAsync("/api/servicos", new CriarServicoRequest
        {
            ProfissionalId = profissionalId,
            CidadeId = cidadeId,
            Titulo = "Servico para dashboard por usuario",
            Descricao = "Teste de drill-down por usuario",
            ValorCombinado = 140m
        });

        Assert.Equal(HttpStatusCode.OK, criarServicoResponse.StatusCode);

        var inicioJanela = Uri.EscapeDataString(DateTime.UtcNow.AddMinutes(-1).ToString("O"));
        var fimJanela = Uri.EscapeDataString(DateTime.UtcNow.AddMinutes(1).ToString("O"));

        var dashboard = await adminClient.GetFromJsonAsync<EmailNotificacaoUsuarioDashboardResponse>(
            $"/api/notificacoes/emails/usuarios/{authProfissional.UsuarioId}/dashboard?tipoNotificacao={TipoNotificacao.ServicoSolicitado}&dataCriacaoInicial={inicioJanela}&dataCriacaoFinal={fimJanela}");

        Assert.NotNull(dashboard);
        Assert.Equal(authProfissional.UsuarioId, dashboard!.UsuarioId);
        Assert.Equal(TipoNotificacao.ServicoSolicitado, dashboard.TipoNotificacao);
        Assert.True(dashboard.Resumo.TotalRegistros > 0);
        Assert.NotEmpty(dashboard.Serie.Itens);
        Assert.NotEmpty(dashboard.Tipos.Itens);
        Assert.NotEmpty(dashboard.Recentes);
        Assert.All(dashboard.Recentes, x => Assert.Equal(authProfissional.UsuarioId, x.UsuarioId));
    }

    [Fact]
    public async Task ReprocessarEmailsOutboxEmLote_DeveProcessarSomenteSubconjuntoFiltrado()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var authCliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "cliente-reproc-lote-email");
        var authProfissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "profissional-reproc-lote-email");
        var authAdmin = await LoginAdminAsync(adminClient);

        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authCliente.Token);
        profissionalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authProfissional.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authAdmin.Token);

        var atualizarResponse = await profissionalClient.PutAsJsonAsync("/api/notificacoes/minhas/preferencias", new AtualizarPreferenciasNotificacaoRequest
        {
            Preferencias =
            [
                new PreferenciaNotificacaoItemRequest
                {
                    Tipo = TipoNotificacao.ServicoSolicitado,
                    AtivoInterno = false,
                    AtivoEmail = true
                }
            ]
        });

        Assert.Equal(HttpStatusCode.OK, atualizarResponse.StatusCode);

        var cidadeId = await _factory.ObterCidadeIdAsync();
        var profissionalId = await _factory.ObterProfissionalIdPorUsuarioIdAsync(authProfissional.UsuarioId);

        var criarServicoResponse = await clienteClient.PostAsJsonAsync("/api/servicos", new CriarServicoRequest
        {
            ProfissionalId = profissionalId,
            CidadeId = cidadeId,
            Titulo = "Servico para reprocessar lote",
            Descricao = "Teste de reprocessamento seletivo",
            ValorCombinado = 150m
        });

        Assert.Equal(HttpStatusCode.OK, criarServicoResponse.StatusCode);

        var reprocessarResponse = await adminClient.PostAsJsonAsync("/api/notificacoes/emails/reprocessar-lote", new AtualizarEmailsOutboxEmLoteRequest
        {
            Status = StatusEmailNotificacao.Pendente,
            UsuarioId = authProfissional.UsuarioId,
            TipoNotificacao = TipoNotificacao.ServicoSolicitado,
            Limite = 10
        });

        var reprocessado = await reprocessarResponse.Content.ReadFromJsonAsync<AtualizarEmailsOutboxEmLoteResponse>();

        Assert.Equal(HttpStatusCode.OK, reprocessarResponse.StatusCode);
        Assert.NotNull(reprocessado);
        Assert.True(reprocessado!.QuantidadeAfetada > 0);

        var enviados = await adminClient.GetFromJsonAsync<PaginacaoResponse<EmailNotificacaoOutboxResponse>>(
            $"/api/notificacoes/emails?status={StatusEmailNotificacao.Enviado}&usuarioId={authProfissional.UsuarioId}&tipoNotificacao={TipoNotificacao.ServicoSolicitado}");

        Assert.NotNull(enviados);
        Assert.NotEmpty(enviados!.Itens);
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
