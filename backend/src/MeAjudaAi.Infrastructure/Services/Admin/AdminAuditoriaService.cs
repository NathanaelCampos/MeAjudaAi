using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.Interfaces.Admin;
using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Infrastructure.Services.Admin;

public class AdminAuditoriaService : IAdminAuditoriaService
{
    private readonly AppDbContext _context;

    public AdminAuditoriaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task RegistrarAsync(
        Guid adminUsuarioId,
        string entidade,
        Guid entidadeId,
        string acao,
        string descricao,
        string? payloadJson = null,
        CancellationToken cancellationToken = default)
    {
        _context.AuditoriasAdminAcoes.Add(new AuditoriaAdminAcao
        {
            AdminUsuarioId = adminUsuarioId,
            Entidade = entidade,
            EntidadeId = entidadeId,
            Acao = acao,
            Descricao = descricao,
            PayloadJson = payloadJson ?? string.Empty
        });

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PaginacaoResponse<AuditoriaAdminListItemResponse>> BuscarAsync(
        BuscarAuditoriasAdminRequest request,
        CancellationToken cancellationToken = default)
    {
        var pagina = request.Pagina < 1 ? 1 : request.Pagina;
        var tamanhoPagina = request.TamanhoPagina < 1 ? 20 : Math.Min(request.TamanhoPagina, 100);

        var query = _context.AuditoriasAdminAcoes
            .AsNoTracking()
            .Include(x => x.AdminUsuario)
            .AsQueryable();

        if (request.AdminUsuarioId.HasValue)
            query = query.Where(x => x.AdminUsuarioId == request.AdminUsuarioId.Value);

        if (!string.IsNullOrWhiteSpace(request.Entidade))
        {
            var entidade = request.Entidade.Trim().ToLower();
            query = query.Where(x => x.Entidade.ToLower() == entidade);
        }

        if (request.EntidadeId.HasValue)
            query = query.Where(x => x.EntidadeId == request.EntidadeId.Value);

        if (!string.IsNullOrWhiteSpace(request.Acao))
        {
            var acao = request.Acao.Trim().ToLower();
            query = query.Where(x => x.Acao.ToLower() == acao);
        }

        if (request.DataCriacaoInicial.HasValue)
            query = query.Where(x => x.DataCriacao >= request.DataCriacaoInicial.Value);

        if (request.DataCriacaoFinal.HasValue)
            query = query.Where(x => x.DataCriacao <= request.DataCriacaoFinal.Value);

        var totalRegistros = await query.CountAsync(cancellationToken);

        var itens = await query
            .OrderByDescending(x => x.DataCriacao)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(x => new AuditoriaAdminListItemResponse
            {
                Id = x.Id,
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

        return new PaginacaoResponse<AuditoriaAdminListItemResponse>
        {
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalRegistros = totalRegistros,
            TotalPaginas = totalRegistros == 0 ? 0 : (int)Math.Ceiling(totalRegistros / (double)tamanhoPagina),
            Itens = itens
        };
    }

    public async Task<AuditoriaAdminDetalheResponse?> ObterPorIdAsync(
        Guid auditoriaId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AuditoriasAdminAcoes
            .AsNoTracking()
            .Include(x => x.AdminUsuario)
            .Where(x => x.Id == auditoriaId)
            .Select(x => new AuditoriaAdminDetalheResponse
            {
                Id = x.Id,
                AdminUsuarioId = x.AdminUsuarioId,
                NomeAdmin = x.AdminUsuario.Nome,
                EmailAdmin = x.AdminUsuario.Email,
                Entidade = x.Entidade,
                EntidadeId = x.EntidadeId,
                Acao = x.Acao,
                Descricao = x.Descricao,
                PayloadJson = x.PayloadJson,
                DataCriacao = x.DataCriacao
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
