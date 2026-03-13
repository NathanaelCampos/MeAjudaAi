using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.DTOs.Impulsionamentos;
using MeAjudaAi.Application.Interfaces.Admin;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Infrastructure.Services.Admin;

public class AdminWebhookPagamentoService : IAdminWebhookPagamentoService
{
    private readonly AppDbContext _context;

    public AdminWebhookPagamentoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaginacaoResponse<WebhookPagamentoImpulsionamentoEventoResponse>> BuscarAsync(
        BuscarWebhooksPagamentoAdminRequest request,
        CancellationToken cancellationToken = default)
    {
        var pagina = request.Pagina < 1 ? 1 : request.Pagina;
        var tamanhoPagina = request.TamanhoPagina < 1 ? 20 : Math.Min(request.TamanhoPagina, 100);

        var query = _context.WebhookPagamentoImpulsionamentoEventos
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.EventoExternoId))
        {
            var eventoExternoId = request.EventoExternoId.Trim().ToLower();
            query = query.Where(x => x.EventoExternoId.ToLower().Contains(eventoExternoId));
        }

        if (!string.IsNullOrWhiteSpace(request.CodigoReferenciaPagamento))
        {
            var codigo = request.CodigoReferenciaPagamento.Trim().ToLower();
            query = query.Where(x => x.CodigoReferenciaPagamento.ToLower().Contains(codigo));
        }

        if (!string.IsNullOrWhiteSpace(request.Provedor))
        {
            var provedor = request.Provedor.Trim().ToLower();
            query = query.Where(x => x.Provedor.ToLower().Contains(provedor));
        }

        if (request.ProcessadoComSucesso.HasValue)
            query = query.Where(x => x.ProcessadoComSucesso == request.ProcessadoComSucesso.Value);

        if (request.ImpulsionamentoProfissionalId.HasValue)
            query = query.Where(x => x.ImpulsionamentoProfissionalId == request.ImpulsionamentoProfissionalId.Value);

        if (request.DataCriacaoInicial.HasValue)
            query = query.Where(x => x.DataCriacao >= request.DataCriacaoInicial.Value);

        if (request.DataCriacaoFinal.HasValue)
            query = query.Where(x => x.DataCriacao <= request.DataCriacaoFinal.Value);

        var totalRegistros = await query.CountAsync(cancellationToken);

        var itens = await query
            .OrderByDescending(x => x.DataCriacao)
            .ThenByDescending(x => x.Id)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(x => new WebhookPagamentoImpulsionamentoEventoResponse
            {
                Id = x.Id,
                Provedor = x.Provedor,
                EventoExternoId = x.EventoExternoId,
                CodigoReferenciaPagamento = x.CodigoReferenciaPagamento,
                StatusPagamento = x.StatusPagamento,
                ProcessadoComSucesso = x.ProcessadoComSucesso,
                MensagemResultado = x.MensagemResultado,
                IpOrigem = x.IpOrigem,
                RequestId = x.RequestId,
                UserAgent = x.UserAgent,
                ImpulsionamentoProfissionalId = x.ImpulsionamentoProfissionalId,
                StatusImpulsionamentoResultado = x.StatusImpulsionamentoResultado.HasValue ? (int)x.StatusImpulsionamentoResultado.Value : null,
                DataCriacao = x.DataCriacao
            })
            .ToListAsync(cancellationToken);

        return new PaginacaoResponse<WebhookPagamentoImpulsionamentoEventoResponse>
        {
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalRegistros = totalRegistros,
            TotalPaginas = totalRegistros == 0 ? 0 : (int)Math.Ceiling(totalRegistros / (double)tamanhoPagina),
            Itens = itens
        };
    }

    public async Task<WebhookPagamentoAdminDetalheResponse?> ObterPorIdAsync(
        Guid webhookId,
        CancellationToken cancellationToken = default)
    {
        return await _context.WebhookPagamentoImpulsionamentoEventos
            .AsNoTracking()
            .Where(x => x.Id == webhookId)
            .Select(x => new WebhookPagamentoAdminDetalheResponse
            {
                Id = x.Id,
                Provedor = x.Provedor,
                EventoExternoId = x.EventoExternoId,
                CodigoReferenciaPagamento = x.CodigoReferenciaPagamento,
                StatusPagamento = x.StatusPagamento,
                ProcessadoComSucesso = x.ProcessadoComSucesso,
                MensagemResultado = x.MensagemResultado,
                PayloadJson = x.PayloadJson,
                HeadersJson = x.HeadersJson,
                IpOrigem = x.IpOrigem,
                RequestId = x.RequestId,
                UserAgent = x.UserAgent,
                ImpulsionamentoProfissionalId = x.ImpulsionamentoProfissionalId,
                StatusImpulsionamentoResultado = x.StatusImpulsionamentoResultado.HasValue ? (int)x.StatusImpulsionamentoResultado.Value : null,
                DataCriacao = x.DataCriacao
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<WebhookPagamentoAdminDashboardResponse?> ObterDashboardAsync(
        Guid webhookId,
        CancellationToken cancellationToken = default)
    {
        var webhook = await ObterPorIdAsync(webhookId, cancellationToken);
        if (webhook is null)
            return null;

        ImpulsionamentoAdminDetalheResponse? impulsionamento = null;
        if (webhook.ImpulsionamentoProfissionalId.HasValue)
        {
            impulsionamento = await _context.ImpulsionamentosProfissionais
                .AsNoTracking()
                .Include(x => x.Profissional).ThenInclude(x => x.Usuario)
                .Include(x => x.PlanoImpulsionamento)
                .Where(x => x.Id == webhook.ImpulsionamentoProfissionalId.Value)
                .Select(x => new ImpulsionamentoAdminDetalheResponse
                {
                    Id = x.Id,
                    ProfissionalId = x.ProfissionalId,
                    PlanoImpulsionamentoId = x.PlanoImpulsionamentoId,
                    NomeProfissional = x.Profissional.NomeExibicao,
                    EmailProfissional = x.Profissional.Usuario.Email,
                    NomePlano = x.PlanoImpulsionamento.Nome,
                    Status = x.Status,
                    DataInicio = x.DataInicio,
                    DataFim = x.DataFim,
                    ValorPago = x.ValorPago,
                    CodigoReferenciaPagamento = x.CodigoReferenciaPagamento,
                    DataCriacao = x.DataCriacao
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        var referenciaId = webhook.ImpulsionamentoProfissionalId;

        var notificacoesQuery = _context.NotificacoesUsuarios
            .AsNoTracking()
            .Where(x => referenciaId.HasValue && x.ReferenciaId == referenciaId.Value);

        var totalNotificacoes = await notificacoesQuery.CountAsync(cancellationToken);
        var naoLidas = await notificacoesQuery.CountAsync(x => x.Ativo && x.DataLeitura == null, cancellationToken);
        var lidas = await notificacoesQuery.CountAsync(x => x.Ativo && x.DataLeitura != null, cancellationToken);
        var arquivadas = await notificacoesQuery.CountAsync(x => !x.Ativo, cancellationToken);

        var emailsQuery = _context.EmailsNotificacoesOutbox
            .AsNoTracking()
            .Where(x => referenciaId.HasValue && x.ReferenciaId == referenciaId.Value);

        var totalEmails = await emailsQuery.CountAsync(cancellationToken);
        var pendentes = await emailsQuery.CountAsync(x => x.Status == Domain.Enums.StatusEmailNotificacao.Pendente, cancellationToken);
        var enviados = await emailsQuery.CountAsync(x => x.Status == Domain.Enums.StatusEmailNotificacao.Enviado, cancellationToken);
        var falhas = await emailsQuery.CountAsync(x => x.Status == Domain.Enums.StatusEmailNotificacao.Falha, cancellationToken);
        var cancelados = await emailsQuery.CountAsync(x => x.Status == Domain.Enums.StatusEmailNotificacao.Cancelado, cancellationToken);
        var ultimoStatus = await emailsQuery
            .OrderByDescending(x => x.DataCriacao)
            .Select(x => (Domain.Enums.StatusEmailNotificacao?)x.Status)
            .FirstOrDefaultAsync(cancellationToken);
        var ultimaDataEmail = await emailsQuery.MaxAsync(x => (DateTime?)x.DataCriacao, cancellationToken);

        var webhooksRelacionadosQuery = _context.WebhookPagamentoImpulsionamentoEventos
            .AsNoTracking()
            .Where(x => x.CodigoReferenciaPagamento == webhook.CodigoReferenciaPagamento);

        var totalWebhooks = await webhooksRelacionadosQuery.CountAsync(cancellationToken);
        var sucessos = await webhooksRelacionadosQuery.CountAsync(x => x.ProcessadoComSucesso, cancellationToken);
        var falhasWebhook = totalWebhooks - sucessos;
        var ultimaDataWebhook = await webhooksRelacionadosQuery.MaxAsync(x => (DateTime?)x.DataCriacao, cancellationToken);
        var recentes = await webhooksRelacionadosQuery
            .OrderByDescending(x => x.DataCriacao)
            .Take(10)
            .Select(x => new WebhookPagamentoImpulsionamentoEventoResponse
            {
                Id = x.Id,
                Provedor = x.Provedor,
                EventoExternoId = x.EventoExternoId,
                CodigoReferenciaPagamento = x.CodigoReferenciaPagamento,
                StatusPagamento = x.StatusPagamento,
                ProcessadoComSucesso = x.ProcessadoComSucesso,
                MensagemResultado = x.MensagemResultado,
                IpOrigem = x.IpOrigem,
                RequestId = x.RequestId,
                UserAgent = x.UserAgent,
                ImpulsionamentoProfissionalId = x.ImpulsionamentoProfissionalId,
                StatusImpulsionamentoResultado = x.StatusImpulsionamentoResultado.HasValue ? (int)x.StatusImpulsionamentoResultado.Value : null,
                DataCriacao = x.DataCriacao
            })
            .ToListAsync(cancellationToken);

        return new WebhookPagamentoAdminDashboardResponse
        {
            Webhook = webhook,
            Impulsionamento = impulsionamento,
            Notificacoes = new UsuarioAdminDashboardNotificacoesResponse
            {
                TotalAtivas = totalNotificacoes - arquivadas,
                NaoLidas = naoLidas,
                Lidas = lidas,
                Arquivadas = arquivadas
            },
            Emails = new UsuarioAdminDashboardEmailsResponse
            {
                Total = totalEmails,
                Pendentes = pendentes,
                Enviados = enviados,
                Falhas = falhas,
                Cancelados = cancelados,
                UltimoStatus = ultimoStatus,
                UltimaDataCriacao = ultimaDataEmail
            },
            WebhooksRelacionados = new ImpulsionamentoAdminDashboardWebhooksResponse
            {
                Total = totalWebhooks,
                Sucessos = sucessos,
                Falhas = falhasWebhook,
                UltimaDataCriacao = ultimaDataWebhook,
                Recentes = recentes
            }
        };
    }
}
