using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.Interfaces.Admin;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Infrastructure.Services.Admin;

public class AdminProfissionalService : IAdminProfissionalService
{
    private readonly AppDbContext _context;

    public AdminProfissionalService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaginacaoResponse<ProfissionalAdminListItemResponse>> BuscarAsync(
        BuscarProfissionaisAdminRequest request,
        CancellationToken cancellationToken = default)
    {
        var pagina = request.Pagina < 1 ? 1 : request.Pagina;
        var tamanhoPagina = request.TamanhoPagina < 1 ? 20 : Math.Min(request.TamanhoPagina, 100);

        var query = _context.Profissionais
            .AsNoTracking()
            .Include(x => x.Usuario)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Nome))
        {
            var nome = request.Nome.Trim().ToLower();
            query = query.Where(x =>
                x.NomeExibicao.ToLower().Contains(nome) ||
                x.Usuario.Nome.ToLower().Contains(nome) ||
                x.Usuario.Email.ToLower().Contains(nome));
        }

        if (request.Ativo.HasValue)
            query = query.Where(x => x.Ativo == request.Ativo.Value);

        if (request.PerfilVerificado.HasValue)
            query = query.Where(x => x.PerfilVerificado == request.PerfilVerificado.Value);

        var totalRegistros = await query.CountAsync(cancellationToken);

        var itens = await query
            .OrderBy(x => x.NomeExibicao)
            .ThenBy(x => x.Usuario.Email)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(x => new ProfissionalAdminListItemResponse
            {
                Id = x.Id,
                UsuarioId = x.UsuarioId,
                NomeExibicao = x.NomeExibicao,
                NomeUsuario = x.Usuario.Nome,
                EmailUsuario = x.Usuario.Email,
                Ativo = x.Ativo,
                PerfilVerificado = x.PerfilVerificado,
                AceitaContatoPeloApp = x.AceitaContatoPeloApp,
                DataCriacao = x.DataCriacao
            })
            .ToListAsync(cancellationToken);

        return new PaginacaoResponse<ProfissionalAdminListItemResponse>
        {
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalRegistros = totalRegistros,
            TotalPaginas = totalRegistros == 0 ? 0 : (int)Math.Ceiling(totalRegistros / (double)tamanhoPagina),
            Itens = itens
        };
    }

    public async Task<ProfissionalAdminDetalheResponse?> ObterPorIdAsync(
        Guid profissionalId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Profissionais
            .AsNoTracking()
            .Include(x => x.Usuario)
            .Where(x => x.Id == profissionalId)
            .Select(x => new ProfissionalAdminDetalheResponse
            {
                Id = x.Id,
                UsuarioId = x.UsuarioId,
                NomeExibicao = x.NomeExibicao,
                NomeUsuario = x.Usuario.Nome,
                EmailUsuario = x.Usuario.Email,
                TelefoneUsuario = x.Usuario.Telefone,
                Descricao = x.Descricao,
                WhatsApp = x.WhatsApp,
                Instagram = x.Instagram,
                Facebook = x.Facebook,
                OutraFormaContato = x.OutraFormaContato,
                Ativo = x.Ativo,
                PerfilVerificado = x.PerfilVerificado,
                AceitaContatoPeloApp = x.AceitaContatoPeloApp,
                DataCriacao = x.DataCriacao,
                NotaMediaAtendimento = x.NotaMediaAtendimento,
                NotaMediaServico = x.NotaMediaServico,
                NotaMediaPreco = x.NotaMediaPreco
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ProfissionalAdminDetalheResponse> DefinirPerfilVerificadoAsync(
        Guid profissionalId,
        bool perfilVerificado,
        CancellationToken cancellationToken = default)
    {
        var profissional = await _context.Profissionais
            .FirstOrDefaultAsync(x => x.Id == profissionalId, cancellationToken);

        if (profissional is null)
            throw new InvalidOperationException("Profissional não encontrado.");

        profissional.PerfilVerificado = perfilVerificado;
        profissional.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return (await ObterPorIdAsync(profissionalId, cancellationToken))!;
    }

    public async Task<ProfissionalAdminDetalheResponse> DefinirAtivoAsync(
        Guid profissionalId,
        bool ativo,
        CancellationToken cancellationToken = default)
    {
        var profissional = await _context.Profissionais
            .FirstOrDefaultAsync(x => x.Id == profissionalId, cancellationToken);

        if (profissional is null)
            throw new InvalidOperationException("Profissional não encontrado.");

        profissional.Ativo = ativo;
        profissional.DataAtualizacao = DateTime.UtcNow;

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == profissional.UsuarioId, cancellationToken);
        if (usuario is not null)
        {
            usuario.Ativo = ativo;
            usuario.DataAtualizacao = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return (await ObterPorIdAsync(profissionalId, cancellationToken))!;
    }
}
