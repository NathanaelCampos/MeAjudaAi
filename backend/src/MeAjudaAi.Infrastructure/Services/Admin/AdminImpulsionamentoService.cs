using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.Interfaces.Admin;
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
}
