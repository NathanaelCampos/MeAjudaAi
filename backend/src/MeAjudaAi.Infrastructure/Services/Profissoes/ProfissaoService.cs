using MeAjudaAi.Application.DTOs.Profissoes;
using MeAjudaAi.Application.Interfaces.Profissoes;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Infrastructure.Services.Profissoes;

public class ProfissaoService : IProfissaoService
{
    private readonly AppDbContext _context;

    public ProfissaoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ProfissaoResponse>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Profissoes
            .AsNoTracking()
            .Where(x => x.Ativo)
            .OrderBy(x => x.Nome)
            .Select(x => new ProfissaoResponse
            {
                Id = x.Id,
                Nome = x.Nome,
                Slug = x.Slug
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EspecialidadeResponse>> ListarEspecialidadesPorProfissaoAsync(
        Guid profissaoId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Especialidades
            .AsNoTracking()
            .Where(x => x.Ativo && x.ProfissaoId == profissaoId)
            .OrderBy(x => x.Nome)
            .Select(x => new EspecialidadeResponse
            {
                Id = x.Id,
                ProfissaoId = x.ProfissaoId,
                Nome = x.Nome
            })
            .ToListAsync(cancellationToken);
    }
}