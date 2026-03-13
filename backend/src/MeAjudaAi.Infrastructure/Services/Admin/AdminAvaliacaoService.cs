using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.Interfaces.Admin;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Infrastructure.Services.Admin;

public class AdminAvaliacaoService : IAdminAvaliacaoService
{
    private readonly AppDbContext _context;

    public AdminAvaliacaoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaginacaoResponse<AvaliacaoAdminListItemResponse>> BuscarAsync(
        BuscarAvaliacoesAdminRequest request,
        CancellationToken cancellationToken = default)
    {
        var pagina = request.Pagina < 1 ? 1 : request.Pagina;
        var tamanhoPagina = request.TamanhoPagina < 1 ? 20 : Math.Min(request.TamanhoPagina, 100);

        var query = _context.Avaliacoes
            .AsNoTracking()
            .Include(x => x.Cliente).ThenInclude(x => x.Usuario)
            .Include(x => x.Profissional).ThenInclude(x => x.Usuario)
            .Include(x => x.Servico)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Termo))
        {
            var termo = request.Termo.Trim().ToLower();
            query = query.Where(x =>
                x.Comentario.ToLower().Contains(termo) ||
                x.Cliente.Usuario.Nome.ToLower().Contains(termo) ||
                x.Cliente.Usuario.Email.ToLower().Contains(termo) ||
                x.Profissional.Usuario.Nome.ToLower().Contains(termo) ||
                x.Profissional.Usuario.Email.ToLower().Contains(termo) ||
                x.Profissional.NomeExibicao.ToLower().Contains(termo) ||
                x.Servico.Titulo.ToLower().Contains(termo));
        }

        if (request.ClienteId.HasValue)
            query = query.Where(x => x.ClienteId == request.ClienteId.Value);

        if (request.ProfissionalId.HasValue)
            query = query.Where(x => x.ProfissionalId == request.ProfissionalId.Value);

        if (request.ServicoId.HasValue)
            query = query.Where(x => x.ServicoId == request.ServicoId.Value);

        if (request.StatusModeracaoComentario.HasValue)
            query = query.Where(x => x.StatusModeracaoComentario == request.StatusModeracaoComentario.Value);

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
            .Select(x => new AvaliacaoAdminListItemResponse
            {
                Id = x.Id,
                ServicoId = x.ServicoId,
                ClienteId = x.ClienteId,
                ProfissionalId = x.ProfissionalId,
                NomeCliente = x.Cliente.Usuario.Nome,
                NomeProfissional = x.Profissional.NomeExibicao,
                Comentario = x.Comentario,
                StatusModeracaoComentario = x.StatusModeracaoComentario,
                DataCriacao = x.DataCriacao
            })
            .ToListAsync(cancellationToken);

        return new PaginacaoResponse<AvaliacaoAdminListItemResponse>
        {
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalRegistros = totalRegistros,
            TotalPaginas = totalRegistros == 0 ? 0 : (int)Math.Ceiling(totalRegistros / (double)tamanhoPagina),
            Itens = itens
        };
    }

    public async Task<AvaliacaoAdminDetalheResponse?> ObterPorIdAsync(
        Guid avaliacaoId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Avaliacoes
            .AsNoTracking()
            .Include(x => x.Cliente).ThenInclude(x => x.Usuario)
            .Include(x => x.Profissional).ThenInclude(x => x.Usuario)
            .Include(x => x.Servico)
            .Where(x => x.Id == avaliacaoId)
            .Select(x => new AvaliacaoAdminDetalheResponse
            {
                Id = x.Id,
                ServicoId = x.ServicoId,
                ClienteId = x.ClienteId,
                ProfissionalId = x.ProfissionalId,
                NomeCliente = x.Cliente.Usuario.Nome,
                EmailCliente = x.Cliente.Usuario.Email,
                NomeProfissional = x.Profissional.NomeExibicao,
                EmailProfissional = x.Profissional.Usuario.Email,
                TituloServico = x.Servico.Titulo,
                NotaAtendimento = x.NotaAtendimento,
                NotaServico = x.NotaServico,
                NotaPreco = x.NotaPreco,
                Comentario = x.Comentario,
                StatusModeracaoComentario = x.StatusModeracaoComentario,
                DataCriacao = x.DataCriacao
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
