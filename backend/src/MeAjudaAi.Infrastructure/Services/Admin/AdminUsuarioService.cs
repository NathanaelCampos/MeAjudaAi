using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.Interfaces.Admin;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Infrastructure.Services.Admin;

public class AdminUsuarioService : IAdminUsuarioService
{
    private readonly AppDbContext _context;

    public AdminUsuarioService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaginacaoResponse<UsuarioAdminListItemResponse>> BuscarAsync(
        BuscarUsuariosAdminRequest request,
        CancellationToken cancellationToken = default)
    {
        var pagina = request.Pagina < 1 ? 1 : request.Pagina;
        var tamanhoPagina = request.TamanhoPagina < 1 ? 20 : Math.Min(request.TamanhoPagina, 100);

        var query = _context.Usuarios
            .AsNoTracking()
            .Include(x => x.Cliente)
            .Include(x => x.Profissional)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Nome))
        {
            var nome = request.Nome.Trim().ToLower();
            query = query.Where(x => x.Nome.ToLower().Contains(nome));
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var email = request.Email.Trim().ToLower();
            query = query.Where(x => x.Email.ToLower().Contains(email));
        }

        if (request.TipoPerfil.HasValue)
            query = query.Where(x => x.TipoPerfil == request.TipoPerfil.Value);

        if (request.Ativo.HasValue)
            query = query.Where(x => x.Ativo == request.Ativo.Value);

        var totalRegistros = await query.CountAsync(cancellationToken);

        var itens = await query
            .OrderBy(x => x.Nome)
            .ThenBy(x => x.Email)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(x => new UsuarioAdminListItemResponse
            {
                Id = x.Id,
                Nome = x.Nome,
                Email = x.Email,
                Telefone = x.Telefone,
                TipoPerfil = x.TipoPerfil,
                Ativo = x.Ativo,
                DataCriacao = x.DataCriacao,
                DataUltimoLogin = x.DataUltimoLogin,
                NomeExibicao = x.TipoPerfil == Domain.Enums.TipoPerfil.Cliente
                    ? (x.Cliente != null ? x.Cliente.NomeExibicao : x.Nome)
                    : x.TipoPerfil == Domain.Enums.TipoPerfil.Profissional
                        ? (x.Profissional != null ? x.Profissional.NomeExibicao : x.Nome)
                        : x.Nome,
                PerfilVerificado = x.Profissional != null ? x.Profissional.PerfilVerificado : null
            })
            .ToListAsync(cancellationToken);

        return new PaginacaoResponse<UsuarioAdminListItemResponse>
        {
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalRegistros = totalRegistros,
            TotalPaginas = totalRegistros == 0 ? 0 : (int)Math.Ceiling(totalRegistros / (double)tamanhoPagina),
            Itens = itens
        };
    }

    public async Task<UsuarioAdminDetalheResponse?> ObterPorIdAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .AsNoTracking()
            .Include(x => x.Cliente)
            .Include(x => x.Profissional)
            .Where(x => x.Id == usuarioId)
            .Select(x => new UsuarioAdminDetalheResponse
            {
                Id = x.Id,
                Nome = x.Nome,
                Email = x.Email,
                Telefone = x.Telefone,
                TipoPerfil = x.TipoPerfil,
                Ativo = x.Ativo,
                DataCriacao = x.DataCriacao,
                DataUltimoLogin = x.DataUltimoLogin,
                ClienteId = x.Cliente != null ? x.Cliente.Id : null,
                ProfissionalId = x.Profissional != null ? x.Profissional.Id : null,
                NomeExibicao = x.TipoPerfil == Domain.Enums.TipoPerfil.Cliente
                    ? (x.Cliente != null ? x.Cliente.NomeExibicao : x.Nome)
                    : x.TipoPerfil == Domain.Enums.TipoPerfil.Profissional
                        ? (x.Profissional != null ? x.Profissional.NomeExibicao : x.Nome)
                        : x.Nome,
                PerfilVerificado = x.Profissional != null ? x.Profissional.PerfilVerificado : null
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UsuarioAdminDetalheResponse> DefinirAtivoAsync(
        Guid usuarioId,
        bool ativo,
        Guid? usuarioAdministradorId = null,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(x => x.Id == usuarioId, cancellationToken);

        if (usuario is null)
            throw new InvalidOperationException("Usuário não encontrado.");

        if (!ativo && usuarioAdministradorId.HasValue && usuarioAdministradorId.Value == usuarioId)
            throw new InvalidOperationException("Não é permitido bloquear o próprio usuário administrador.");

        usuario.Ativo = ativo;
        usuario.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return (await ObterPorIdAsync(usuarioId, cancellationToken))!;
    }
}
