using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.Interfaces.Admin;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Infrastructure.Configurations;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Infrastructure.Services.Admin;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly AppDbContext _context;
    private readonly AdminDashboardOptions _options;

    public AdminDashboardService(AppDbContext context, IOptions<AdminDashboardOptions> options)
    {
        _context = context;
        _options = options.Value;
    }

    public async Task<AdminDashboardResponse> ObterAsync(
        BuscarAdminDashboardRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        var hoje = DateTime.UtcNow.Date;
        var agora = DateTime.UtcNow;
        var presetPeriodo = NormalizarPresetPeriodo(request?.PresetPeriodo);
        var (janelaQualidadePreset, janelaAcaoAdminPreset, janelaSeriePreset) = ObterJanelasDoPreset(presetPeriodo);
        var presetAnterior = ObterPresetAnterior(presetPeriodo);
        var (_, _, janelaSerieAnteriorPreset) = ObterJanelasDoPreset(presetAnterior);

        var janelaQualidadeDias = request?.JanelaQualidadeDias is > 0
            ? request.JanelaQualidadeDias.Value
            : janelaQualidadePreset ?? _options.JanelaQualidadeDias;
        var janelaAcaoAdminRecenteHoras = request?.JanelaAcaoAdminRecenteHoras is > 0
            ? request.JanelaAcaoAdminRecenteHoras.Value
            : janelaAcaoAdminPreset ?? _options.JanelaAcaoAdminRecenteHoras;
        var janelaSerieDias = request?.JanelaSerieDias is > 0
            ? request.JanelaSerieDias.Value
            : janelaSeriePreset ?? _options.JanelaSerieDias;
        var janelaAcaoAdminRecente = TimeSpan.FromHours(janelaAcaoAdminRecenteHoras);
        var inicioJanelaAcaoAdminRecente = agora.Subtract(janelaAcaoAdminRecente);
        var janelaQualidadeOperacional = TimeSpan.FromDays(janelaQualidadeDias);
        var inicioJanelaQualidade = agora.Subtract(janelaQualidadeOperacional);
        var inicioJanelaSerie = hoje.AddDays(-(janelaSerieDias - 1));
        var inicioJanelaAnterior = inicioJanelaSerie.AddDays(-janelaSerieDias);
        var fimJanelaAnterior = inicioJanelaSerie.AddDays(-1);
        var janelaSerieComparativoDias = janelaSerieAnteriorPreset ?? 0;
        var inicioComparativoPresetAnterior = janelaSerieComparativoDias > 0
            ? inicioJanelaSerie.AddDays(-janelaSerieComparativoDias)
            : (DateTime?)null;
        var fimComparativoPresetAnterior = janelaSerieComparativoDias > 0
            ? inicioJanelaSerie.AddDays(-1)
            : (DateTime?)null;

        var totalUsuarios = await _context.Usuarios.CountAsync(cancellationToken);
        var usuariosAtivos = await _context.Usuarios.CountAsync(x => x.Ativo, cancellationToken);
        var clientes = await _context.Usuarios.CountAsync(x => x.TipoPerfil == TipoPerfil.Cliente, cancellationToken);
        var profissionais = await _context.Usuarios.CountAsync(x => x.TipoPerfil == TipoPerfil.Profissional, cancellationToken);
        var administradores = await _context.Usuarios.CountAsync(x => x.TipoPerfil == TipoPerfil.Administrador, cancellationToken);

        var totalProfissionais = await _context.Profissionais.CountAsync(cancellationToken);
        var profissionaisAtivos = await _context.Profissionais.CountAsync(x => x.Ativo, cancellationToken);
        var profissionaisVerificados = await _context.Profissionais.CountAsync(x => x.PerfilVerificado, cancellationToken);

        var totalServicos = await _context.Servicos.CountAsync(cancellationToken);
        var servicosSolicitados = await _context.Servicos.CountAsync(x => x.Status == StatusServico.Solicitado, cancellationToken);
        var servicosAceitos = await _context.Servicos.CountAsync(x => x.Status == StatusServico.Aceito, cancellationToken);
        var servicosEmExecucao = await _context.Servicos.CountAsync(x => x.Status == StatusServico.EmExecucao, cancellationToken);
        var servicosConcluidos = await _context.Servicos.CountAsync(x => x.Status == StatusServico.Concluido, cancellationToken);
        var servicosCancelados = await _context.Servicos.CountAsync(x => x.Status == StatusServico.Cancelado, cancellationToken);

        var totalAvaliacoes = await _context.Avaliacoes.CountAsync(cancellationToken);
        var avaliacoesPendentes = await _context.Avaliacoes.CountAsync(x => x.StatusModeracaoComentario == StatusModeracaoComentario.Pendente, cancellationToken);
        var avaliacoesAprovadas = await _context.Avaliacoes.CountAsync(x => x.StatusModeracaoComentario == StatusModeracaoComentario.Aprovado, cancellationToken);
        var avaliacoesRejeitadas = await _context.Avaliacoes.CountAsync(x => x.StatusModeracaoComentario == StatusModeracaoComentario.Rejeitado, cancellationToken);
        var avaliacoesOcultas = await _context.Avaliacoes.CountAsync(x => x.StatusModeracaoComentario == StatusModeracaoComentario.Oculto, cancellationToken);

        var totalImpulsionamentos = await _context.ImpulsionamentosProfissionais.CountAsync(cancellationToken);
        var impulsionamentosPendentes = await _context.ImpulsionamentosProfissionais.CountAsync(x => x.Status == StatusImpulsionamento.PendentePagamento, cancellationToken);
        var impulsionamentosAtivos = await _context.ImpulsionamentosProfissionais.CountAsync(x => x.Status == StatusImpulsionamento.Ativo, cancellationToken);
        var impulsionamentosEncerrados = await _context.ImpulsionamentosProfissionais.CountAsync(x => x.Status == StatusImpulsionamento.Encerrado, cancellationToken);
        var impulsionamentosCancelados = await _context.ImpulsionamentosProfissionais.CountAsync(x => x.Status == StatusImpulsionamento.Cancelado, cancellationToken);
        var impulsionamentosExpirados = await _context.ImpulsionamentosProfissionais.CountAsync(x => x.Status == StatusImpulsionamento.Expirado, cancellationToken);

        var totalWebhooks = await _context.WebhookPagamentoImpulsionamentoEventos.CountAsync(cancellationToken);
        var webhooksSucesso = await _context.WebhookPagamentoImpulsionamentoEventos.CountAsync(x => x.ProcessadoComSucesso, cancellationToken);
        var totalWebhooksRecentes = await _context.WebhookPagamentoImpulsionamentoEventos.CountAsync(
            x => x.DataCriacao >= inicioJanelaQualidade,
            cancellationToken);
        var webhooksSucessoRecentes = await _context.WebhookPagamentoImpulsionamentoEventos.CountAsync(
            x => x.DataCriacao >= inicioJanelaQualidade && x.ProcessadoComSucesso,
            cancellationToken);
        var ultimaDataWebhook = await _context.WebhookPagamentoImpulsionamentoEventos.MaxAsync(x => (DateTime?)x.DataCriacao, cancellationToken);

        var notificacoesAtivas = await _context.NotificacoesUsuarios.CountAsync(x => x.Ativo, cancellationToken);
        var notificacoesNaoLidas = await _context.NotificacoesUsuarios.CountAsync(x => x.Ativo && x.DataLeitura == null, cancellationToken);
        var notificacoesLidas = await _context.NotificacoesUsuarios.CountAsync(x => x.Ativo && x.DataLeitura != null, cancellationToken);
        var notificacoesArquivadas = await _context.NotificacoesUsuarios.CountAsync(x => !x.Ativo, cancellationToken);

        var totalEmails = await _context.EmailsNotificacoesOutbox.CountAsync(cancellationToken);
        var emailsPendentes = await _context.EmailsNotificacoesOutbox.CountAsync(x => x.Status == StatusEmailNotificacao.Pendente, cancellationToken);
        var emailsEnviados = await _context.EmailsNotificacoesOutbox.CountAsync(x => x.Status == StatusEmailNotificacao.Enviado, cancellationToken);
        var emailsFalhas = await _context.EmailsNotificacoesOutbox.CountAsync(x => x.Status == StatusEmailNotificacao.Falha, cancellationToken);
        var emailsCancelados = await _context.EmailsNotificacoesOutbox.CountAsync(x => x.Status == StatusEmailNotificacao.Cancelado, cancellationToken);
        var totalEmailsRecentes = await _context.EmailsNotificacoesOutbox.CountAsync(
            x => x.DataCriacao >= inicioJanelaQualidade,
            cancellationToken);
        var emailsEnviadosRecentes = await _context.EmailsNotificacoesOutbox.CountAsync(
            x => x.DataCriacao >= inicioJanelaQualidade && x.Status == StatusEmailNotificacao.Enviado,
            cancellationToken);
        var emailsFalhasRecentes = await _context.EmailsNotificacoesOutbox.CountAsync(
            x => x.DataCriacao >= inicioJanelaQualidade && x.Status == StatusEmailNotificacao.Falha,
            cancellationToken);
        var emailsPendentesAtrasados = await _context.EmailsNotificacoesOutbox.CountAsync(
            x => x.Status == StatusEmailNotificacao.Pendente &&
                 x.ProximaTentativaEm != null &&
                 x.ProximaTentativaEm <= agora,
            cancellationToken);
        var ultimoStatusEmail = await _context.EmailsNotificacoesOutbox
            .OrderByDescending(x => x.DataCriacao)
            .Select(x => (StatusEmailNotificacao?)x.Status)
            .FirstOrDefaultAsync(cancellationToken);
        var ultimaDataEmail = await _context.EmailsNotificacoesOutbox.MaxAsync(x => (DateTime?)x.DataCriacao, cancellationToken);
        var ultimoEmailProcessadoEm = await _context.EmailsNotificacoesOutbox
            .Where(x => x.DataProcessamento != null)
            .MaxAsync(x => x.DataProcessamento, cancellationToken);
        var ultimaAcaoAdminEm = await _context.AuditoriasAdminAcoes.MaxAsync(x => (DateTime?)x.DataCriacao, cancellationToken);
        var ultimoWebhookFalhoEm = await _context.WebhookPagamentoImpulsionamentoEventos
            .Where(x => !x.ProcessadoComSucesso)
            .MaxAsync(x => (DateTime?)x.DataCriacao, cancellationToken);

        var serieServicos = await _context.Servicos
            .AsNoTracking()
            .Where(x => x.DataCriacao.Date >= inicioJanelaSerie)
            .GroupBy(x => x.DataCriacao.Date)
            .Select(x => new AdminDashboardSerieItemResponse
            {
                Data = x.Key,
                Total = x.Count()
            })
            .OrderBy(x => x.Data)
            .ToListAsync(cancellationToken);

        var serieAvaliacoes = await _context.Avaliacoes
            .AsNoTracking()
            .Where(x => x.DataCriacao.Date >= inicioJanelaSerie)
            .GroupBy(x => x.DataCriacao.Date)
            .Select(x => new AdminDashboardSerieItemResponse
            {
                Data = x.Key,
                Total = x.Count()
            })
            .OrderBy(x => x.Data)
            .ToListAsync(cancellationToken);

        var serieWebhooks = await _context.WebhookPagamentoImpulsionamentoEventos
            .AsNoTracking()
            .Where(x => x.DataCriacao.Date >= inicioJanelaSerie)
            .GroupBy(x => x.DataCriacao.Date)
            .Select(x => new AdminDashboardSerieItemResponse
            {
                Data = x.Key,
                Total = x.Count()
            })
            .OrderBy(x => x.Data)
            .ToListAsync(cancellationToken);

        var serieEmails = await _context.EmailsNotificacoesOutbox
            .AsNoTracking()
            .Where(x => x.DataCriacao.Date >= inicioJanelaSerie)
            .GroupBy(x => x.DataCriacao.Date)
            .Select(x => new AdminDashboardSerieItemResponse
            {
                Data = x.Key,
                Total = x.Count()
            })
            .OrderBy(x => x.Data)
            .ToListAsync(cancellationToken);

        var servicosUltimos7Dias = await _context.Servicos.CountAsync(x => x.DataCriacao.Date >= inicioJanelaSerie, cancellationToken);
        var servicosSeteDiasAnteriores = await _context.Servicos.CountAsync(x => x.DataCriacao.Date >= inicioJanelaAnterior && x.DataCriacao.Date <= fimJanelaAnterior, cancellationToken);

        var avaliacoesUltimos7Dias = await _context.Avaliacoes.CountAsync(x => x.DataCriacao.Date >= inicioJanelaSerie, cancellationToken);
        var avaliacoesSeteDiasAnteriores = await _context.Avaliacoes.CountAsync(x => x.DataCriacao.Date >= inicioJanelaAnterior && x.DataCriacao.Date <= fimJanelaAnterior, cancellationToken);

        var webhooksUltimos7Dias = await _context.WebhookPagamentoImpulsionamentoEventos.CountAsync(x => x.DataCriacao.Date >= inicioJanelaSerie, cancellationToken);
        var webhooksSeteDiasAnteriores = await _context.WebhookPagamentoImpulsionamentoEventos.CountAsync(x => x.DataCriacao.Date >= inicioJanelaAnterior && x.DataCriacao.Date <= fimJanelaAnterior, cancellationToken);

        var emailsUltimos7Dias = await _context.EmailsNotificacoesOutbox.CountAsync(x => x.DataCriacao.Date >= inicioJanelaSerie, cancellationToken);
        var emailsSeteDiasAnteriores = await _context.EmailsNotificacoesOutbox.CountAsync(x => x.DataCriacao.Date >= inicioJanelaAnterior && x.DataCriacao.Date <= fimJanelaAnterior, cancellationToken);

        var servicosPresetAnterior = inicioComparativoPresetAnterior.HasValue && fimComparativoPresetAnterior.HasValue
            ? await _context.Servicos.CountAsync(
                x => x.DataCriacao.Date >= inicioComparativoPresetAnterior.Value &&
                     x.DataCriacao.Date <= fimComparativoPresetAnterior.Value,
                cancellationToken)
            : 0;
        var avaliacoesPresetAnterior = inicioComparativoPresetAnterior.HasValue && fimComparativoPresetAnterior.HasValue
            ? await _context.Avaliacoes.CountAsync(
                x => x.DataCriacao.Date >= inicioComparativoPresetAnterior.Value &&
                     x.DataCriacao.Date <= fimComparativoPresetAnterior.Value,
                cancellationToken)
            : 0;
        var webhooksPresetAnterior = inicioComparativoPresetAnterior.HasValue && fimComparativoPresetAnterior.HasValue
            ? await _context.WebhookPagamentoImpulsionamentoEventos.CountAsync(
                x => x.DataCriacao.Date >= inicioComparativoPresetAnterior.Value &&
                     x.DataCriacao.Date <= fimComparativoPresetAnterior.Value,
                cancellationToken)
            : 0;
        var emailsPresetAnterior = inicioComparativoPresetAnterior.HasValue && fimComparativoPresetAnterior.HasValue
            ? await _context.EmailsNotificacoesOutbox.CountAsync(
                x => x.DataCriacao.Date >= inicioComparativoPresetAnterior.Value &&
                     x.DataCriacao.Date <= fimComparativoPresetAnterior.Value,
                cancellationToken)
            : 0;

        var webhooksFalhosRecentes = await _context.WebhookPagamentoImpulsionamentoEventos
            .AsNoTracking()
            .Where(x => !x.ProcessadoComSucesso && x.DataCriacao >= inicioJanelaQualidade)
            .OrderByDescending(x => x.DataCriacao)
            .Take(5)
            .Select(x => new AdminDashboardWebhookFalhoItemResponse
            {
                Id = x.Id,
                Provedor = x.Provedor,
                EventoExternoId = x.EventoExternoId,
                CodigoReferenciaPagamento = x.CodigoReferenciaPagamento,
                MensagemResultado = x.MensagemResultado,
                DataCriacao = x.DataCriacao
            })
            .ToListAsync(cancellationToken);

        var emailsFalhosRecentes = await _context.EmailsNotificacoesOutbox
            .AsNoTracking()
            .Where(x => x.Status == StatusEmailNotificacao.Falha && x.DataCriacao >= inicioJanelaQualidade)
            .OrderByDescending(x => x.DataCriacao)
            .Take(5)
            .Select(x => new AdminDashboardEmailFalhoItemResponse
            {
                Id = x.Id,
                UsuarioId = x.UsuarioId,
                EmailDestino = x.EmailDestino,
                TipoNotificacao = x.TipoNotificacao,
                Status = x.Status,
                UltimaMensagemErro = x.UltimaMensagemErro,
                DataCriacao = x.DataCriacao
            })
            .ToListAsync(cancellationToken);

        var avaliacoesPendentesRecentes = await _context.Avaliacoes
            .AsNoTracking()
            .Where(x => x.StatusModeracaoComentario == StatusModeracaoComentario.Pendente)
            .OrderByDescending(x => x.DataCriacao)
            .Take(5)
            .Select(x => new AdminDashboardAvaliacaoPendenteItemResponse
            {
                Id = x.Id,
                ServicoId = x.ServicoId,
                NomeCliente = x.Servico.Cliente.Usuario.Nome,
                NomeProfissional = x.Servico.Profissional.NomeExibicao,
                DataCriacao = x.DataCriacao
            })
            .ToListAsync(cancellationToken);

        var servicosSolicitadosPorProfissional = await _context.Servicos
            .AsNoTracking()
            .Where(x => x.Status == StatusServico.Solicitado)
            .GroupBy(x => x.ProfissionalId)
            .Select(x => new { ProfissionalId = x.Key, Total = x.Count() })
            .ToDictionaryAsync(x => x.ProfissionalId, x => x.Total, cancellationToken);

        var avaliacoesPendentesPorProfissional = await _context.Avaliacoes
            .AsNoTracking()
            .Where(x => x.StatusModeracaoComentario == StatusModeracaoComentario.Pendente)
            .GroupBy(x => x.Servico.ProfissionalId)
            .Select(x => new { ProfissionalId = x.Key, Total = x.Count() })
            .ToDictionaryAsync(x => x.ProfissionalId, x => x.Total, cancellationToken);

        var impulsionamentosPendentesPorProfissional = await _context.ImpulsionamentosProfissionais
            .AsNoTracking()
            .Where(x => x.Status == StatusImpulsionamento.PendentePagamento)
            .GroupBy(x => x.ProfissionalId)
            .Select(x => new { ProfissionalId = x.Key, Total = x.Count() })
            .ToDictionaryAsync(x => x.ProfissionalId, x => x.Total, cancellationToken);

        var webhooksFalhosPorProfissional = await _context.WebhookPagamentoImpulsionamentoEventos
            .AsNoTracking()
            .Where(x => !x.ProcessadoComSucesso &&
                        x.ImpulsionamentoProfissionalId != null &&
                        x.DataCriacao >= inicioJanelaQualidade)
            .GroupBy(x => x.ImpulsionamentoProfissional!.ProfissionalId)
            .Select(x => new { ProfissionalId = x.Key, Total = x.Count() })
            .ToDictionaryAsync(x => x.ProfissionalId, x => x.Total, cancellationToken);

        var emailsFalhosPorProfissional = await _context.EmailsNotificacoesOutbox
            .AsNoTracking()
            .Where(x => x.Status == StatusEmailNotificacao.Falha && x.DataCriacao >= inicioJanelaQualidade)
            .Join(
                _context.Profissionais.AsNoTracking(),
                email => email.UsuarioId,
                profissional => profissional.UsuarioId,
                (email, profissional) => new { ProfissionalId = profissional.Id, EmailId = email.Id })
            .GroupBy(x => x.ProfissionalId)
            .Select(x => new { ProfissionalId = x.Key, Total = x.Count() })
            .ToDictionaryAsync(x => x.ProfissionalId, x => x.Total, cancellationToken);

        var profissionaisEmAtencao = await _context.Profissionais
            .AsNoTracking()
            .Include(x => x.Usuario)
            .ToListAsync(cancellationToken);

        var topProfissionaisEmAtencao = profissionaisEmAtencao
            .Select(profissional =>
            {
                var totalServicosSolicitados = servicosSolicitadosPorProfissional.GetValueOrDefault(profissional.Id);
                var totalAvaliacoesPendentes = avaliacoesPendentesPorProfissional.GetValueOrDefault(profissional.Id);
                var totalImpulsionamentosPendentes = impulsionamentosPendentesPorProfissional.GetValueOrDefault(profissional.Id);
                var totalWebhooksFalhos = webhooksFalhosPorProfissional.GetValueOrDefault(profissional.Id);
                var totalEmailsFalhos = emailsFalhosPorProfissional.GetValueOrDefault(profissional.Id);
                var scoreAtencao = totalServicosSolicitados +
                                   totalAvaliacoesPendentes +
                                   totalImpulsionamentosPendentes +
                                   totalWebhooksFalhos +
                                   totalEmailsFalhos;

                return new AdminDashboardProfissionalEmAtencaoItemResponse
                {
                    ProfissionalId = profissional.Id,
                    UsuarioId = profissional.UsuarioId,
                    NomeExibicao = profissional.NomeExibicao,
                    Email = profissional.Usuario.Email,
                    ServicosSolicitados = totalServicosSolicitados,
                    AvaliacoesPendentes = totalAvaliacoesPendentes,
                    ImpulsionamentosPendentesPagamento = totalImpulsionamentosPendentes,
                    WebhooksFalhos = totalWebhooksFalhos,
                    EmailsComFalha = totalEmailsFalhos,
                    ScoreAtencao = scoreAtencao
                };
            })
            .Where(x => x.ScoreAtencao > 0)
            .OrderByDescending(x => x.ScoreAtencao)
            .ThenBy(x => x.NomeExibicao)
            .Take(5)
            .ToList();

        var servicosEmAbertoPorCliente = await _context.Servicos
            .AsNoTracking()
            .Where(x => x.Status == StatusServico.Solicitado || x.Status == StatusServico.Aceito || x.Status == StatusServico.EmExecucao)
            .GroupBy(x => x.ClienteId)
            .Select(x => new { ClienteId = x.Key, Total = x.Count() })
            .ToDictionaryAsync(x => x.ClienteId, x => x.Total, cancellationToken);

        var notificacoesNaoLidasPorUsuario = await _context.NotificacoesUsuarios
            .AsNoTracking()
            .Where(x => x.Ativo && x.DataLeitura == null)
            .GroupBy(x => x.UsuarioId)
            .Select(x => new { UsuarioId = x.Key, Total = x.Count() })
            .ToDictionaryAsync(x => x.UsuarioId, x => x.Total, cancellationToken);

        var emailsFalhosPorUsuario = await _context.EmailsNotificacoesOutbox
            .AsNoTracking()
            .Where(x => x.Status == StatusEmailNotificacao.Falha && x.DataCriacao >= inicioJanelaQualidade)
            .GroupBy(x => x.UsuarioId)
            .Select(x => new { UsuarioId = x.Key, Total = x.Count() })
            .ToDictionaryAsync(x => x.UsuarioId, x => x.Total, cancellationToken);

        var clientesEmAtencao = await _context.Clientes
            .AsNoTracking()
            .Include(x => x.Usuario)
            .ToListAsync(cancellationToken);

        var topClientesEmAtencao = clientesEmAtencao
            .Select(cliente =>
            {
                var totalServicosEmAberto = servicosEmAbertoPorCliente.GetValueOrDefault(cliente.Id);
                var totalNotificacoesNaoLidas = notificacoesNaoLidasPorUsuario.GetValueOrDefault(cliente.UsuarioId);
                var totalEmailsFalhos = emailsFalhosPorUsuario.GetValueOrDefault(cliente.UsuarioId);
                var scoreAtencao = totalServicosEmAberto + totalNotificacoesNaoLidas + totalEmailsFalhos;

                return new AdminDashboardClienteEmAtencaoItemResponse
                {
                    ClienteId = cliente.Id,
                    UsuarioId = cliente.UsuarioId,
                    NomeExibicao = cliente.NomeExibicao,
                    Email = cliente.Usuario.Email,
                    ServicosEmAberto = totalServicosEmAberto,
                    NotificacoesNaoLidas = totalNotificacoesNaoLidas,
                    EmailsComFalha = totalEmailsFalhos,
                    ScoreAtencao = scoreAtencao
                };
            })
            .Where(x => x.ScoreAtencao > 0)
            .OrderByDescending(x => x.ScoreAtencao)
            .ThenBy(x => x.NomeExibicao)
            .Take(5)
            .ToList();

        var topUsuariosInativosRecentes = await _context.Usuarios
            .AsNoTracking()
            .Where(x => !x.Ativo)
            .OrderByDescending(x => x.DataAtualizacao ?? x.DataCriacao)
            .Take(5)
            .Select(x => new AdminDashboardUsuarioInativoRecenteItemResponse
            {
                UsuarioId = x.Id,
                Nome = x.Nome,
                Email = x.Email,
                TipoPerfil = x.TipoPerfil,
                DataAtualizacao = x.DataAtualizacao
            })
            .ToListAsync(cancellationToken);

        var acoesAdminRecentes = await _context.AuditoriasAdminAcoes
            .AsNoTracking()
            .Where(x => x.DataCriacao >= inicioJanelaAcaoAdminRecente)
            .Include(x => x.AdminUsuario)
            .OrderByDescending(x => x.DataCriacao)
            .Take(5)
            .Select(x => new AdminDashboardAuditoriaAdminItemResponse
            {
                AuditoriaId = x.Id,
                AdminUsuarioId = x.AdminUsuarioId,
                NomeAdmin = x.AdminUsuario.Nome,
                EmailAdmin = x.AdminUsuario.Email,
                Entidade = x.Entidade,
                EntidadeId = x.EntidadeId,
                Acao = x.Acao,
                Descricao = x.Descricao,
                DataCriacao = x.DataCriacao
            })
            .ToListAsync(cancellationToken);

        var topAdminsAtivos = await _context.AuditoriasAdminAcoes
            .AsNoTracking()
            .Where(x => x.DataCriacao >= inicioJanelaAcaoAdminRecente)
            .Include(x => x.AdminUsuario)
            .GroupBy(x => new { x.AdminUsuarioId, x.AdminUsuario.Nome, x.AdminUsuario.Email })
            .Select(x => new AdminDashboardAdminAtivoItemResponse
            {
                AdminUsuarioId = x.Key.AdminUsuarioId,
                NomeAdmin = x.Key.Nome,
                EmailAdmin = x.Key.Email,
                TotalAcoes = x.Count(),
                UltimaAcaoEm = x.Max(y => (DateTime?)y.DataCriacao)
            })
            .OrderByDescending(x => x.TotalAcoes)
            .ThenByDescending(x => x.UltimaAcaoEm)
            .Take(5)
            .ToListAsync(cancellationToken);

        var webhooksFalhosRecentesQuantidade = totalWebhooksRecentes - webhooksSucessoRecentes;
        var riscoOperacional = CalcularRiscoOperacional(
            webhooksFalhosRecentesQuantidade,
            emailsFalhasRecentes,
            emailsPendentesAtrasados,
            avaliacoesPendentes,
            impulsionamentosPendentes,
            servicosSolicitados);

        var semAcaoAdminRecenteSobRisco = riscoOperacional == "alto" &&
                                          (ultimaAcaoAdminEm == null || ultimaAcaoAdminEm <= agora.Subtract(janelaAcaoAdminRecente));

        var acoesRecomendadas = CriarAcoesRecomendadas(
            webhooksFalhosRecentesQuantidade,
            emailsFalhasRecentes,
            emailsPendentesAtrasados,
            avaliacoesPendentes,
            impulsionamentosPendentes,
            servicosSolicitados,
            semAcaoAdminRecenteSobRisco);

        var destinoOperacionalPrimario = ObterDestinoOperacionalPrimario(
            webhooksFalhosRecentesQuantidade,
            emailsFalhasRecentes,
            emailsPendentesAtrasados,
            avaliacoesPendentes,
            impulsionamentosPendentes,
            servicosSolicitados,
            semAcaoAdminRecenteSobRisco);
        var linkOperacionalSugerido = ObterLinkOperacionalSugerido(destinoOperacionalPrimario);

        var comparativoServicos = CriarComparativoPreset(servicosUltimos7Dias, servicosPresetAnterior);
        var comparativoAvaliacoes = CriarComparativoPreset(avaliacoesUltimos7Dias, avaliacoesPresetAnterior);
        var comparativoWebhooks = CriarComparativoPreset(webhooksUltimos7Dias, webhooksPresetAnterior);
        var comparativoEmails = CriarComparativoPreset(emailsUltimos7Dias, emailsPresetAnterior);
        var resumoComparativoPreset = CriarResumoComparativoPreset(
            presetAnterior != null,
            comparativoServicos,
            comparativoAvaliacoes,
            comparativoWebhooks,
            comparativoEmails);
        var statusComparativoPrincipal = CriarStatusComparativoPreset(resumoComparativoPreset);
        var insightComparativoPrincipal = CriarInsightComparativoPreset(resumoComparativoPreset, statusComparativoPrincipal);
        var indicadorComparativoPrincipal = CriarIndicadorComparativoPreset(statusComparativoPrincipal);
        var prioridadeComparativaPrincipal = CriarPrioridadeComparativaPreset(statusComparativoPrincipal);
        var acaoComparativaPrincipal = CriarAcaoComparativaPreset(resumoComparativoPreset);
        var linkComparativoPrincipal = CriarLinkComparativoPreset(resumoComparativoPreset.EixoPrincipal);
        var tooltipComparativoPrincipal = CriarTooltipComparativoPreset(resumoComparativoPreset);

        return new AdminDashboardResponse
        {
            Configuracao = new AdminDashboardConfiguracaoResponse
            {
                PresetPeriodo = presetPeriodo ?? "custom",
                JanelaQualidadeDias = janelaQualidadeDias,
                JanelaAcaoAdminRecenteHoras = janelaAcaoAdminRecenteHoras,
                JanelaSerieDias = janelaSerieDias
            },
            Usuarios = new AdminDashboardUsuariosResponse
            {
                Total = totalUsuarios,
                Ativos = usuariosAtivos,
                Inativos = totalUsuarios - usuariosAtivos,
                Clientes = clientes,
                Profissionais = profissionais,
                Administradores = administradores
            },
            Profissionais = new AdminDashboardProfissionaisResponse
            {
                Total = totalProfissionais,
                Ativos = profissionaisAtivos,
                Inativos = totalProfissionais - profissionaisAtivos,
                Verificados = profissionaisVerificados
            },
            Servicos = new AdminDashboardServicosResponse
            {
                Total = totalServicos,
                Solicitados = servicosSolicitados,
                Aceitos = servicosAceitos,
                EmExecucao = servicosEmExecucao,
                Concluidos = servicosConcluidos,
                Cancelados = servicosCancelados
            },
            Avaliacoes = new AdminDashboardAvaliacoesResponse
            {
                Total = totalAvaliacoes,
                Pendentes = avaliacoesPendentes,
                Aprovadas = avaliacoesAprovadas,
                Rejeitadas = avaliacoesRejeitadas,
                Ocultas = avaliacoesOcultas
            },
            Impulsionamentos = new AdminDashboardImpulsionamentosResponse
            {
                Total = totalImpulsionamentos,
                PendentesPagamento = impulsionamentosPendentes,
                Ativos = impulsionamentosAtivos,
                Encerrados = impulsionamentosEncerrados,
                Cancelados = impulsionamentosCancelados,
                Expirados = impulsionamentosExpirados
            },
            Webhooks = new AdminDashboardWebhooksResponse
            {
                Total = totalWebhooks,
                Sucessos = webhooksSucesso,
                Falhas = totalWebhooks - webhooksSucesso,
                UltimaDataCriacao = ultimaDataWebhook
            },
            Notificacoes = new AdminDashboardNotificacoesResponse
            {
                TotalAtivas = notificacoesAtivas,
                NaoLidas = notificacoesNaoLidas,
                Lidas = notificacoesLidas,
                Arquivadas = notificacoesArquivadas
            },
            Emails = new AdminDashboardEmailsResponse
            {
                Total = totalEmails,
                Pendentes = emailsPendentes,
                Enviados = emailsEnviados,
                Falhas = emailsFalhas,
                Cancelados = emailsCancelados,
                UltimoStatus = ultimoStatusEmail,
                UltimaDataCriacao = ultimaDataEmail
            },
            Series = new AdminDashboardSeriesResponse
            {
                Servicos = serieServicos,
                Avaliacoes = serieAvaliacoes,
                Webhooks = serieWebhooks,
                Emails = serieEmails
            },
            Tendencias = new AdminDashboardTendenciasResponse
            {
                Servicos = CriarTendencia(servicosUltimos7Dias, servicosSeteDiasAnteriores),
                Avaliacoes = CriarTendencia(avaliacoesUltimos7Dias, avaliacoesSeteDiasAnteriores),
                Webhooks = CriarTendencia(webhooksUltimos7Dias, webhooksSeteDiasAnteriores),
                Emails = CriarTendencia(emailsUltimos7Dias, emailsSeteDiasAnteriores)
            },
            ComparativoPresetAnterior = new AdminDashboardComparativoPresetResponse
            {
                Disponivel = presetAnterior != null,
                PresetAtual = presetPeriodo ?? "custom",
                PresetAnterior = presetAnterior,
                JanelaAtualDias = janelaSerieDias,
                JanelaAnteriorDias = janelaSerieComparativoDias,
                Servicos = comparativoServicos,
                Avaliacoes = comparativoAvaliacoes,
                Webhooks = comparativoWebhooks,
                Emails = comparativoEmails,
                Resumo = resumoComparativoPreset
            },
            InsightComparativoPrincipal = insightComparativoPrincipal,
            EixoComparativoPrincipal = resumoComparativoPreset.EixoPrincipal,
            VariacaoComparativaPrincipal = presetAnterior != null
                ? resumoComparativoPreset.VariacaoPrincipalPercentual
                : null,
            DirecaoComparativaPrincipal = resumoComparativoPreset.DirecaoPrincipal,
            StatusComparativoPrincipal = statusComparativoPrincipal,
            IndicadorComparativoPrincipal = indicadorComparativoPrincipal,
            PrioridadeComparativaPrincipal = prioridadeComparativaPrincipal,
            AcaoComparativaPrincipal = acaoComparativaPrincipal,
            LinkComparativoPrincipal = linkComparativoPrincipal,
            TooltipComparativoPrincipal = tooltipComparativoPrincipal,
            Pendencias = new AdminDashboardPendenciasResponse
            {
                AvaliacoesPendentesModeracao = avaliacoesPendentes,
                ImpulsionamentosPendentesPagamento = impulsionamentosPendentes,
                ServicosSolicitados = servicosSolicitados,
                NotificacoesNaoLidas = notificacoesNaoLidas,
                EmailsPendentes = emailsPendentes
            },
            Alertas = new AdminDashboardAlertasResponse
            {
                WebhooksFalhos = webhooksFalhosRecentesQuantidade,
                EmailsComFalha = emailsFalhasRecentes,
                EmailsPendentesAtrasados = emailsPendentesAtrasados,
                SemAcaoAdminRecenteSobRisco = semAcaoAdminRecenteSobRisco,
                UltimaAcaoAdminEm = ultimaAcaoAdminEm
            },
            RiscoOperacional = riscoOperacional,
            ItensCriticosRecentes = new AdminDashboardItensCriticosRecentesResponse
            {
                WebhooksFalhos = webhooksFalhosRecentes,
                EmailsFalhos = emailsFalhosRecentes,
                AvaliacoesPendentes = avaliacoesPendentesRecentes
            },
            AcoesRecomendadas = new AdminDashboardAcoesRecomendadasResponse
            {
                Itens = acoesRecomendadas
            },
            TopProfissionaisEmAtencao = topProfissionaisEmAtencao,
            TopClientesEmAtencao = topClientesEmAtencao,
            TopUsuariosInativosRecentes = topUsuariosInativosRecentes,
            AcoesAdminRecentes = acoesAdminRecentes,
            TopAdminsAtivos = topAdminsAtivos,
            SlaOperacional = new AdminDashboardSlaOperacionalResponse
            {
                UltimaAcaoAdminEm = ultimaAcaoAdminEm,
                MinutosDesdeUltimaAcaoAdmin = CalcularMinutosDesde(ultimaAcaoAdminEm, agora),
                UltimoWebhookFalhoEm = ultimoWebhookFalhoEm,
                MinutosDesdeUltimoWebhookFalho = CalcularMinutosDesde(ultimoWebhookFalhoEm, agora),
                UltimoEmailProcessadoEm = ultimoEmailProcessadoEm,
                MinutosDesdeUltimoEmailProcessado = CalcularMinutosDesde(ultimoEmailProcessadoEm, agora)
            },
            DisponibilidadeOperacional = new AdminDashboardDisponibilidadeOperacionalResponse
            {
                PercentualSucessoWebhooks = CalcularPercentual(totalWebhooksRecentes, webhooksSucessoRecentes),
                PercentualFalhaWebhooks = CalcularPercentual(totalWebhooksRecentes, webhooksFalhosRecentesQuantidade),
                PercentualSucessoEmails = CalcularPercentual(totalEmailsRecentes, emailsEnviadosRecentes),
                PercentualFalhaEmails = CalcularPercentual(totalEmailsRecentes, emailsFalhasRecentes)
            },
            SaudeOperacional = CriarSaudeOperacional(
                riscoOperacional,
                semAcaoAdminRecenteSobRisco,
                CalcularPercentual(totalWebhooksRecentes, webhooksFalhosRecentesQuantidade),
                CalcularPercentual(totalEmailsRecentes, emailsFalhasRecentes),
                acoesRecomendadas,
                destinoOperacionalPrimario,
                linkOperacionalSugerido),
            ResumoDecisorio = CriarResumoDecisorio(
                webhooksFalhosRecentesQuantidade,
                emailsFalhasRecentes,
                emailsPendentesAtrasados,
                avaliacoesPendentes,
                impulsionamentosPendentes,
                servicosSolicitados,
                semAcaoAdminRecenteSobRisco)
        };
    }

    private static string? NormalizarPresetPeriodo(string? presetPeriodo)
    {
        if (string.IsNullOrWhiteSpace(presetPeriodo))
        {
            return null;
        }

        return presetPeriodo.Trim().ToLowerInvariant() switch
        {
            "7d" => "7d",
            "15d" => "15d",
            "30d" => "30d",
            _ => null
        };
    }

    private static (int? JanelaQualidadeDias, int? JanelaAcaoAdminRecenteHoras, int? JanelaSerieDias) ObterJanelasDoPreset(string? presetPeriodo)
    {
        return presetPeriodo switch
        {
            "7d" => (7, 24, 7),
            "15d" => (15, 48, 15),
            "30d" => (30, 72, 30),
            _ => (null, null, null)
        };
    }

    private static string? ObterPresetAnterior(string? presetPeriodo)
    {
        return presetPeriodo switch
        {
            "30d" => "15d",
            "15d" => "7d",
            _ => null
        };
    }

    private static AdminDashboardTendenciaItemResponse CriarTendencia(int ultimos7Dias, int seteDiasAnteriores)
    {
        decimal variacaoPercentual;

        if (seteDiasAnteriores == 0)
            variacaoPercentual = ultimos7Dias == 0 ? 0 : 100;
        else
            variacaoPercentual = Math.Round(((ultimos7Dias - seteDiasAnteriores) / (decimal)seteDiasAnteriores) * 100m, 2);

        return new AdminDashboardTendenciaItemResponse
        {
            Ultimos7Dias = ultimos7Dias,
            SeteDiasAnteriores = seteDiasAnteriores,
            VariacaoPercentual = variacaoPercentual
        };
    }

    private static AdminDashboardComparativoPresetItemResponse CriarComparativoPreset(int totalPresetAtual, int totalPresetAnterior)
    {
        decimal variacaoPercentual;

        if (totalPresetAnterior == 0)
            variacaoPercentual = totalPresetAtual == 0 ? 0 : 100;
        else
            variacaoPercentual = Math.Round(((totalPresetAtual - totalPresetAnterior) / (decimal)totalPresetAnterior) * 100m, 2);

        return new AdminDashboardComparativoPresetItemResponse
        {
            TotalPresetAtual = totalPresetAtual,
            TotalPresetAnterior = totalPresetAnterior,
            VariacaoPercentual = variacaoPercentual
        };
    }

    private static AdminDashboardResumoComparativoPresetResponse CriarResumoComparativoPreset(
        bool disponivel,
        AdminDashboardComparativoPresetItemResponse servicos,
        AdminDashboardComparativoPresetItemResponse avaliacoes,
        AdminDashboardComparativoPresetItemResponse webhooks,
        AdminDashboardComparativoPresetItemResponse emails)
    {
        if (!disponivel)
        {
            return new AdminDashboardResumoComparativoPresetResponse
            {
                Disponivel = false,
                DirecaoPrincipal = "indisponivel",
                Resumo = "Sem preset anterior comparavel.",
                Recomendacao = "Selecione um preset padrao para habilitar o comparativo."
            };
        }

        var principal = new List<(string Eixo, AdminDashboardComparativoPresetItemResponse Item)>
        {
            ("servicos", servicos),
            ("avaliacoes", avaliacoes),
            ("webhooks", webhooks),
            ("emails", emails)
        }
        .OrderByDescending(x => Math.Abs(x.Item.VariacaoPercentual))
        .First();

        var direcaoPrincipal = principal.Item.VariacaoPercentual switch
        {
            > 0 => "alta",
            < 0 => "queda",
            _ => "estavel"
        };

        var resumo = direcaoPrincipal switch
        {
            "alta" => $"Crescimento mais forte em {principal.Eixo}.",
            "queda" => $"Reducao mais forte em {principal.Eixo}.",
            _ => "Comparativo entre presets sem variacao relevante."
        };

        var recomendacao = principal.Eixo switch
        {
            "webhooks" when direcaoPrincipal == "alta" => "Verificar se o aumento de webhooks acompanha o processamento esperado.",
            "emails" when direcaoPrincipal == "alta" => "Acompanhar o crescimento do outbox e a capacidade de envio.",
            "servicos" when direcaoPrincipal == "alta" => "Avaliar capacidade operacional para o aumento de servicos.",
            "avaliacoes" when direcaoPrincipal == "alta" => "Acompanhar fila de moderacao para absorver o aumento de avaliacoes.",
            "webhooks" when direcaoPrincipal == "queda" => "Confirmar se houve queda de volume ou mudanca no fluxo de pagamentos.",
            "emails" when direcaoPrincipal == "queda" => "Verificar se a queda do outbox reflete melhora operacional ou perda de disparos.",
            "servicos" when direcaoPrincipal == "queda" => "Monitorar reducao de demanda e impactos na operacao.",
            "avaliacoes" when direcaoPrincipal == "queda" => "Confirmar se a queda de avaliacoes acompanha menor volume de servicos.",
            _ => "Manter acompanhamento da variacao entre presets."
        };

        return new AdminDashboardResumoComparativoPresetResponse
        {
            Disponivel = true,
            EixoPrincipal = principal.Eixo,
            VariacaoPrincipalPercentual = principal.Item.VariacaoPercentual,
            DirecaoPrincipal = direcaoPrincipal,
            Resumo = resumo,
            Recomendacao = recomendacao
        };
    }

    private static AdminDashboardInsightComparativoPresetResponse CriarInsightComparativoPreset(
        AdminDashboardResumoComparativoPresetResponse resumoComparativo,
        string statusComparativo)
    {
        if (!resumoComparativo.Disponivel)
        {
            return new AdminDashboardInsightComparativoPresetResponse
            {
                Disponivel = false,
                Titulo = "Comparativo entre presets indisponivel",
                Detalhe = resumoComparativo.Resumo,
                Recomendacao = resumoComparativo.Recomendacao
            };
        }

        var titulo = statusComparativo switch
        {
            "positivo" => $"Comparativo favoravel com destaque para {resumoComparativo.EixoPrincipal}",
            "negativo" => $"Comparativo exige atencao em {resumoComparativo.EixoPrincipal}",
            _ => $"Comparativo estavel com destaque para {resumoComparativo.EixoPrincipal}"
        };

        return new AdminDashboardInsightComparativoPresetResponse
        {
            Disponivel = true,
            Titulo = titulo,
            Detalhe = resumoComparativo.Resumo,
            Recomendacao = resumoComparativo.Recomendacao
        };
    }

    private static string CriarStatusComparativoPreset(AdminDashboardResumoComparativoPresetResponse resumoComparativo)
    {
        if (!resumoComparativo.Disponivel || resumoComparativo.DirecaoPrincipal == "estavel")
            return "neutro";

        var eixosVolumePositivo = new HashSet<string>(StringComparer.Ordinal)
        {
            "servicos",
            "avaliacoes"
        };

        var altaEhPositiva = eixosVolumePositivo.Contains(resumoComparativo.EixoPrincipal);

        return resumoComparativo.DirecaoPrincipal switch
        {
            "alta" => altaEhPositiva ? "positivo" : "negativo",
            "queda" => altaEhPositiva ? "negativo" : "positivo",
            _ => "neutro"
        };
    }

    private static string CriarIndicadorComparativoPreset(string statusComparativo)
    {
        return statusComparativo switch
        {
            "positivo" => "verde",
            "negativo" => "vermelho",
            _ => "amarelo"
        };
    }

    private static string CriarPrioridadeComparativaPreset(string statusComparativo)
    {
        return statusComparativo switch
        {
            "positivo" => "baixa",
            "negativo" => "alta",
            _ => "media"
        };
    }

    private static string CriarAcaoComparativaPreset(AdminDashboardResumoComparativoPresetResponse resumoComparativo)
    {
        if (!resumoComparativo.Disponivel)
            return "Selecionar preset comparavel";

        return resumoComparativo.EixoPrincipal switch
        {
            "webhooks" => "Revisar operacao de webhooks",
            "emails" => "Revisar operacao de emails",
            "servicos" => "Revisar capacidade de atendimento",
            "avaliacoes" => "Revisar fila de moderacao",
            _ => "Revisar comparativo entre presets"
        };
    }

    private static string CriarLinkComparativoPreset(string eixoPrincipal)
    {
        return eixoPrincipal switch
        {
            "webhooks" => "/admin/webhooks/pagamentos",
            "emails" => "/admin/notificacoes/emails",
            "servicos" => "/admin/servicos",
            "avaliacoes" => "/admin/avaliacoes",
            _ => "/admin/dashboard"
        };
    }

    private static string CriarTooltipComparativoPreset(AdminDashboardResumoComparativoPresetResponse resumoComparativo)
    {
        if (!resumoComparativo.Disponivel)
            return "Comparativo indisponivel sem preset anterior equivalente.";

        return $"{resumoComparativo.Resumo} {resumoComparativo.Recomendacao}";
    }

    private static string CalcularRiscoOperacional(
        int webhooksFalhos,
        int emailsFalhas,
        int emailsPendentesAtrasados,
        int avaliacoesPendentes,
        int impulsionamentosPendentes,
        int servicosSolicitados)
    {
        if (webhooksFalhos > 0 || emailsFalhas > 0 || emailsPendentesAtrasados > 0 || avaliacoesPendentes >= 5)
            return "alto";

        if (avaliacoesPendentes > 0 || impulsionamentosPendentes > 0 || servicosSolicitados > 0)
            return "medio";

        return "baixo";
    }

    private static List<string> CriarAcoesRecomendadas(
        int webhooksFalhos,
        int emailsFalhas,
        int emailsPendentesAtrasados,
        int avaliacoesPendentes,
        int impulsionamentosPendentes,
        int servicosSolicitados,
        bool semAcaoAdminRecenteSobRisco)
    {
        var itens = new List<string>();

        if (semAcaoAdminRecenteSobRisco)
            itens.Add("Acionar administracao para tratar backlog critico sem intervencao recente.");

        if (webhooksFalhos > 0)
            itens.Add("Revisar webhooks de pagamento com falha.");

        if (emailsFalhas > 0)
            itens.Add("Reprocessar emails com falha no outbox.");

        if (emailsPendentesAtrasados > 0)
            itens.Add("Verificar emails pendentes atrasados.");

        if (avaliacoesPendentes > 0)
            itens.Add("Moderar avaliacoes pendentes.");

        if (impulsionamentosPendentes > 0)
            itens.Add("Validar pagamentos pendentes de impulsionamento.");

        if (servicosSolicitados > 0)
            itens.Add("Acompanhar servicos ainda em status solicitado.");

        if (itens.Count == 0)
            itens.Add("Operacao estavel sem acoes imediatas.");

        return itens;
    }

    private static AdminDashboardResumoDecisorioResponse CriarResumoDecisorio(
        int webhooksFalhos,
        int emailsFalhas,
        int emailsPendentesAtrasados,
        int avaliacoesPendentes,
        int impulsionamentosPendentes,
        int servicosSolicitados,
        bool semAcaoAdminRecenteSobRisco)
    {
        var situacaoGeral = CalcularRiscoOperacional(
            webhooksFalhos,
            emailsFalhas,
            emailsPendentesAtrasados,
            avaliacoesPendentes,
            impulsionamentosPendentes,
            servicosSolicitados);

        var gargalos = new List<(string Nome, int Valor)>
        {
            ("webhooks com falha", webhooksFalhos),
            ("emails com falha", emailsFalhas),
            ("emails pendentes atrasados", emailsPendentesAtrasados),
            ("avaliacoes pendentes", avaliacoesPendentes),
            ("impulsionamentos pendentes de pagamento", impulsionamentosPendentes),
            ("servicos solicitados", servicosSolicitados)
        };

        var principal = gargalos
            .OrderByDescending(x => x.Valor)
            .First();

        var focoPrincipal = principal.Valor > 0 ? principal.Nome : "operacao estavel";
        var principalGargalo = principal.Valor > 0
            ? $"{principal.Valor} registro(s) em {principal.Nome}"
            : "Sem gargalo operacional relevante.";

        if (semAcaoAdminRecenteSobRisco)
            principalGargalo = $"{principalGargalo} Sem acao administrativa recente para esse nivel de risco.";

        var recomendacaoImediata = CriarAcoesRecomendadas(
            webhooksFalhos,
            emailsFalhas,
            emailsPendentesAtrasados,
            avaliacoesPendentes,
            impulsionamentosPendentes,
            servicosSolicitados,
            semAcaoAdminRecenteSobRisco)[0];

        return new AdminDashboardResumoDecisorioResponse
        {
            SituacaoGeral = situacaoGeral,
            FocoPrincipal = focoPrincipal,
            PrincipalGargalo = principalGargalo,
            RecomendacaoImediata = recomendacaoImediata
        };
    }

    private static int? CalcularMinutosDesde(DateTime? dataReferencia, DateTime agora)
    {
        if (dataReferencia == null)
            return null;

        return Math.Max(0, (int)Math.Floor((agora - dataReferencia.Value).TotalMinutes));
    }

    private static decimal CalcularPercentual(int total, int parcela)
    {
        if (total <= 0)
            return 0;

        return Math.Round((parcela / (decimal)total) * 100m, 2);
    }

    private static AdminDashboardSaudeOperacionalResponse CriarSaudeOperacional(
        string riscoOperacional,
        bool semAcaoAdminRecenteSobRisco,
        decimal percentualFalhaWebhooks,
        decimal percentualFalhaEmails,
        List<string> acoesRecomendadas,
        string destinoOperacionalPrimario,
        string linkOperacionalSugerido)
    {
        var acaoPrimariaSugerida = acoesRecomendadas.FirstOrDefault() ?? "Operacao estavel sem acoes imediatas.";

        if (semAcaoAdminRecenteSobRisco)
        {
            return new AdminDashboardSaudeOperacionalResponse
            {
                Status = "critico",
                IndicadorCor = "vermelho",
                PrioridadeVisual = "alta",
                OrdemAtencao = 1,
                AcaoPrimariaSugerida = acaoPrimariaSugerida,
                DestinoOperacionalPrimario = destinoOperacionalPrimario,
                LinkOperacionalSugerido = linkOperacionalSugerido,
                Resumo = "Risco alto sem acao administrativa recente."
            };
        }

        if (riscoOperacional == "alto" || percentualFalhaWebhooks >= 20m || percentualFalhaEmails >= 20m)
        {
            return new AdminDashboardSaudeOperacionalResponse
            {
                Status = "critico",
                IndicadorCor = "vermelho",
                PrioridadeVisual = "alta",
                OrdemAtencao = 1,
                AcaoPrimariaSugerida = acaoPrimariaSugerida,
                DestinoOperacionalPrimario = destinoOperacionalPrimario,
                LinkOperacionalSugerido = linkOperacionalSugerido,
                Resumo = "Operacao com falhas relevantes em canais ou backlog critico."
            };
        }

        if (riscoOperacional == "medio" || percentualFalhaWebhooks > 0m || percentualFalhaEmails > 0m)
        {
            return new AdminDashboardSaudeOperacionalResponse
            {
                Status = "atencao",
                IndicadorCor = "amarelo",
                PrioridadeVisual = "media",
                OrdemAtencao = 2,
                AcaoPrimariaSugerida = acaoPrimariaSugerida,
                DestinoOperacionalPrimario = destinoOperacionalPrimario,
                LinkOperacionalSugerido = linkOperacionalSugerido,
                Resumo = "Operacao sob atencao com pendencias ou falhas pontuais."
            };
        }

        return new AdminDashboardSaudeOperacionalResponse
        {
            Status = "saudavel",
            IndicadorCor = "verde",
            PrioridadeVisual = "baixa",
            OrdemAtencao = 3,
            AcaoPrimariaSugerida = acaoPrimariaSugerida,
            DestinoOperacionalPrimario = destinoOperacionalPrimario,
            LinkOperacionalSugerido = linkOperacionalSugerido,
            Resumo = "Operacao estavel com sinais controlados."
        };
    }

    private static string ObterDestinoOperacionalPrimario(
        int webhooksFalhos,
        int emailsFalhas,
        int emailsPendentesAtrasados,
        int avaliacoesPendentes,
        int impulsionamentosPendentes,
        int servicosSolicitados,
        bool semAcaoAdminRecenteSobRisco)
    {
        if (semAcaoAdminRecenteSobRisco)
            return "auditoria-admin";

        var gargalos = new List<(string Destino, int Valor)>
        {
            ("webhooks", webhooksFalhos),
            ("emails", emailsFalhas + emailsPendentesAtrasados),
            ("avaliacoes", avaliacoesPendentes),
            ("impulsionamentos", impulsionamentosPendentes),
            ("servicos", servicosSolicitados)
        };

        var principal = gargalos
            .OrderByDescending(x => x.Valor)
            .First();

        return principal.Valor > 0 ? principal.Destino : "dashboard";
    }

    private static string ObterLinkOperacionalSugerido(string destinoOperacionalPrimario)
    {
        return destinoOperacionalPrimario switch
        {
            "auditoria-admin" => "/admin/auditoria",
            "webhooks" => "/admin/webhooks/pagamentos",
            "emails" => "/admin/notificacoes/emails",
            "avaliacoes" => "/admin/avaliacoes",
            "impulsionamentos" => "/admin/impulsionamentos",
            "servicos" => "/admin/servicos",
            _ => "/admin/dashboard"
        };
    }
}
