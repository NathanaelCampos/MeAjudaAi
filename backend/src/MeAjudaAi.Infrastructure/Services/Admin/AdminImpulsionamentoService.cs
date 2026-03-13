using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.DTOs.Impulsionamentos;
using MeAjudaAi.Application.Interfaces.Admin;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Infrastructure.Services.Admin;

public class AdminImpulsionamentoService : IAdminImpulsionamentoService
{
    private readonly AppDbContext _context;

    public AdminImpulsionamentoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaginacaoResponse<ImpulsionamentoAdminListItemResponse>> BuscarAsync(
        BuscarImpulsionamentosAdminRequest request,
        CancellationToken cancellationToken = default)
    {
        var pagina = request.Pagina < 1 ? 1 : request.Pagina;
        var tamanhoPagina = request.TamanhoPagina < 1 ? 20 : Math.Min(request.TamanhoPagina, 100);

        var query = _context.ImpulsionamentosProfissionais
            .AsNoTracking()
            .Include(x => x.Profissional).ThenInclude(x => x.Usuario)
            .Include(x => x.PlanoImpulsionamento)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Termo))
        {
            var termo = request.Termo.Trim().ToLower();
            query = query.Where(x =>
                x.Profissional.NomeExibicao.ToLower().Contains(termo) ||
                x.Profissional.Usuario.Email.ToLower().Contains(termo) ||
                x.PlanoImpulsionamento.Nome.ToLower().Contains(termo) ||
                x.CodigoReferenciaPagamento.ToLower().Contains(termo));
        }

        if (request.ProfissionalId.HasValue)
            query = query.Where(x => x.ProfissionalId == request.ProfissionalId.Value);

        if (request.PlanoImpulsionamentoId.HasValue)
            query = query.Where(x => x.PlanoImpulsionamentoId == request.PlanoImpulsionamentoId.Value);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (request.DataInicioInicial.HasValue)
            query = query.Where(x => x.DataInicio >= request.DataInicioInicial.Value);

        if (request.DataInicioFinal.HasValue)
            query = query.Where(x => x.DataInicio <= request.DataInicioFinal.Value);

        var totalRegistros = await query.CountAsync(cancellationToken);

        var itens = await query
            .OrderByDescending(x => x.DataCriacao)
            .ThenByDescending(x => x.Id)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(x => new ImpulsionamentoAdminListItemResponse
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
                CodigoReferenciaPagamento = x.CodigoReferenciaPagamento
            })
            .ToListAsync(cancellationToken);

        return new PaginacaoResponse<ImpulsionamentoAdminListItemResponse>
        {
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalRegistros = totalRegistros,
            TotalPaginas = totalRegistros == 0 ? 0 : (int)Math.Ceiling(totalRegistros / (double)tamanhoPagina),
            Itens = itens
        };
    }

    public async Task<ImpulsionamentoAdminDetalheResponse?> ObterPorIdAsync(
        Guid impulsionamentoId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ImpulsionamentosProfissionais
            .AsNoTracking()
            .Include(x => x.Profissional).ThenInclude(x => x.Usuario)
            .Include(x => x.PlanoImpulsionamento)
            .Where(x => x.Id == impulsionamentoId)
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

    public async Task<ImpulsionamentoAdminDashboardResponse?> ObterDashboardAsync(
        Guid impulsionamentoId,
        CancellationToken cancellationToken = default)
    {
        var impulsionamento = await ObterPorIdAsync(impulsionamentoId, cancellationToken);
        if (impulsionamento is null)
            return null;

        var notificacoesQuery = _context.NotificacoesUsuarios
            .AsNoTracking()
            .Where(x => x.ReferenciaId == impulsionamentoId);

        var totalNotificacoes = await notificacoesQuery.CountAsync(cancellationToken);
        var naoLidas = await notificacoesQuery.CountAsync(x => x.Ativo && x.DataLeitura == null, cancellationToken);
        var lidas = await notificacoesQuery.CountAsync(x => x.Ativo && x.DataLeitura != null, cancellationToken);
        var arquivadas = await notificacoesQuery.CountAsync(x => !x.Ativo, cancellationToken);

        var emailsQuery = _context.EmailsNotificacoesOutbox
            .AsNoTracking()
            .Where(x => x.ReferenciaId == impulsionamentoId);

        var totalEmails = await emailsQuery.CountAsync(cancellationToken);
        var pendentes = await emailsQuery.CountAsync(x => x.Status == StatusEmailNotificacao.Pendente, cancellationToken);
        var enviados = await emailsQuery.CountAsync(x => x.Status == StatusEmailNotificacao.Enviado, cancellationToken);
        var falhas = await emailsQuery.CountAsync(x => x.Status == StatusEmailNotificacao.Falha, cancellationToken);
        var cancelados = await emailsQuery.CountAsync(x => x.Status == StatusEmailNotificacao.Cancelado, cancellationToken);
        var ultimoStatus = await emailsQuery
            .OrderByDescending(x => x.DataCriacao)
            .Select(x => (StatusEmailNotificacao?)x.Status)
            .FirstOrDefaultAsync(cancellationToken);
        var ultimaDataEmail = await emailsQuery.MaxAsync(x => (DateTime?)x.DataCriacao, cancellationToken);

        var webhooksQuery = _context.WebhookPagamentoImpulsionamentoEventos
            .AsNoTracking()
            .Where(x => x.CodigoReferenciaPagamento == impulsionamento.CodigoReferenciaPagamento);

        var totalWebhooks = await webhooksQuery.CountAsync(cancellationToken);
        var sucessos = await webhooksQuery.CountAsync(x => x.ProcessadoComSucesso, cancellationToken);
        var falhasWebhook = totalWebhooks - sucessos;
        var ultimaDataWebhook = await webhooksQuery.MaxAsync(x => (DateTime?)x.DataCriacao, cancellationToken);
        var recentes = await webhooksQuery
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

        return new ImpulsionamentoAdminDashboardResponse
        {
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
            Webhooks = new ImpulsionamentoAdminDashboardWebhooksResponse
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
