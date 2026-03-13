using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.Interfaces.Admin;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Infrastructure.Services.Admin;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly AppDbContext _context;

    public AdminDashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDashboardResponse> ObterAsync(CancellationToken cancellationToken = default)
    {
        var hoje = DateTime.UtcNow.Date;
        var agora = DateTime.UtcNow;
        var inicioUltimos7Dias = hoje.AddDays(-6);
        var inicioSeteDiasAnteriores = hoje.AddDays(-13);
        var fimSeteDiasAnteriores = hoje.AddDays(-7);

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

        var serieServicos = await _context.Servicos
            .AsNoTracking()
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
            .GroupBy(x => x.DataCriacao.Date)
            .Select(x => new AdminDashboardSerieItemResponse
            {
                Data = x.Key,
                Total = x.Count()
            })
            .OrderBy(x => x.Data)
            .ToListAsync(cancellationToken);

        var servicosUltimos7Dias = await _context.Servicos.CountAsync(x => x.DataCriacao.Date >= inicioUltimos7Dias, cancellationToken);
        var servicosSeteDiasAnteriores = await _context.Servicos.CountAsync(x => x.DataCriacao.Date >= inicioSeteDiasAnteriores && x.DataCriacao.Date <= fimSeteDiasAnteriores, cancellationToken);

        var avaliacoesUltimos7Dias = await _context.Avaliacoes.CountAsync(x => x.DataCriacao.Date >= inicioUltimos7Dias, cancellationToken);
        var avaliacoesSeteDiasAnteriores = await _context.Avaliacoes.CountAsync(x => x.DataCriacao.Date >= inicioSeteDiasAnteriores && x.DataCriacao.Date <= fimSeteDiasAnteriores, cancellationToken);

        var webhooksUltimos7Dias = await _context.WebhookPagamentoImpulsionamentoEventos.CountAsync(x => x.DataCriacao.Date >= inicioUltimos7Dias, cancellationToken);
        var webhooksSeteDiasAnteriores = await _context.WebhookPagamentoImpulsionamentoEventos.CountAsync(x => x.DataCriacao.Date >= inicioSeteDiasAnteriores && x.DataCriacao.Date <= fimSeteDiasAnteriores, cancellationToken);

        var emailsUltimos7Dias = await _context.EmailsNotificacoesOutbox.CountAsync(x => x.DataCriacao.Date >= inicioUltimos7Dias, cancellationToken);
        var emailsSeteDiasAnteriores = await _context.EmailsNotificacoesOutbox.CountAsync(x => x.DataCriacao.Date >= inicioSeteDiasAnteriores && x.DataCriacao.Date <= fimSeteDiasAnteriores, cancellationToken);

        var webhooksFalhosRecentes = await _context.WebhookPagamentoImpulsionamentoEventos
            .AsNoTracking()
            .Where(x => !x.ProcessadoComSucesso)
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
            .Where(x => x.Status == StatusEmailNotificacao.Falha)
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
            .Where(x => !x.ProcessadoComSucesso && x.ImpulsionamentoProfissionalId != null)
            .GroupBy(x => x.ImpulsionamentoProfissional!.ProfissionalId)
            .Select(x => new { ProfissionalId = x.Key, Total = x.Count() })
            .ToDictionaryAsync(x => x.ProfissionalId, x => x.Total, cancellationToken);

        var emailsFalhosPorProfissional = await _context.EmailsNotificacoesOutbox
            .AsNoTracking()
            .Where(x => x.Status == StatusEmailNotificacao.Falha)
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
            .Where(x => x.Status == StatusEmailNotificacao.Falha)
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

        return new AdminDashboardResponse
        {
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
                WebhooksFalhos = totalWebhooks - webhooksSucesso,
                EmailsComFalha = emailsFalhas,
                EmailsPendentesAtrasados = emailsPendentesAtrasados
            },
            RiscoOperacional = CalcularRiscoOperacional(
                totalWebhooks - webhooksSucesso,
                emailsFalhas,
                emailsPendentesAtrasados,
                avaliacoesPendentes,
                impulsionamentosPendentes,
                servicosSolicitados),
            ItensCriticosRecentes = new AdminDashboardItensCriticosRecentesResponse
            {
                WebhooksFalhos = webhooksFalhosRecentes,
                EmailsFalhos = emailsFalhosRecentes,
                AvaliacoesPendentes = avaliacoesPendentesRecentes
            },
            AcoesRecomendadas = new AdminDashboardAcoesRecomendadasResponse
            {
                Itens = CriarAcoesRecomendadas(
                    totalWebhooks - webhooksSucesso,
                    emailsFalhas,
                    emailsPendentesAtrasados,
                    avaliacoesPendentes,
                    impulsionamentosPendentes,
                    servicosSolicitados)
            },
            TopProfissionaisEmAtencao = topProfissionaisEmAtencao,
            TopClientesEmAtencao = topClientesEmAtencao,
            TopUsuariosInativosRecentes = topUsuariosInativosRecentes
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
        int servicosSolicitados)
    {
        var itens = new List<string>();

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
}
