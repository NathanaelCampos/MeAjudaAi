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
}
