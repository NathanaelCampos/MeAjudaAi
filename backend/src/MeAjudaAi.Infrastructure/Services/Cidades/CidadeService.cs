using MeAjudaAi.Application.DTOs.Cidades;
using MeAjudaAi.Application.Interfaces.Cidades;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Infrastructure.Services.Cidades;

public class CidadeService : ICidadeService
{
    private readonly AppDbContext _context;

    public CidadeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CidadeResponse>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Cidades
            .AsNoTracking()
            .Where(x => x.Ativo)
            .OrderBy(x => x.Estado.UF)
            .ThenBy(x => x.Nome)
            .Select(x => new CidadeResponse
            {
                Id = x.Id,
                EstadoId = x.EstadoId,
                Nome = x.Nome,
                UF = x.Estado.UF,
                CodigoIbge = x.CodigoIbge
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BairroResponse>> ListarBairrosPorCidadeAsync(
        Guid cidadeId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Bairros
            .AsNoTracking()
            .Where(x => x.Ativo && x.CidadeId == cidadeId)
            .OrderBy(x => x.Nome)
            .Select(x => new BairroResponse
            {
                Id = x.Id,
                CidadeId = x.CidadeId,
                Nome = x.Nome
            })
            .ToListAsync(cancellationToken);
    }
}