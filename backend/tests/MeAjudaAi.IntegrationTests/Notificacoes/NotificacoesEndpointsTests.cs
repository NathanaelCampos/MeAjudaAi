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
