using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.Interfaces.Admin;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Infrastructure.Services.Admin;

public class AdminServicoService : IAdminServicoService
{
    private readonly AppDbContext _context;

    public AdminServicoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaginacaoResponse<ServicoAdminListItemResponse>> BuscarAsync(
        BuscarServicosAdminRequest request,
        CancellationToken cancellationToken = default)
    {
        var pagina = request.Pagina < 1 ? 1 : request.Pagina;
        var tamanhoPagina = request.TamanhoPagina < 1 ? 20 : Math.Min(request.TamanhoPagina, 100);

        var query = _context.Servicos
            .AsNoTracking()
            .Include(x => x.Cliente).ThenInclude(x => x.Usuario)
            .Include(x => x.Profissional).ThenInclude(x => x.Usuario)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Termo))
        {
            var termo = request.Termo.Trim().ToLower();
            query = query.Where(x =>
                x.Titulo.ToLower().Contains(termo) ||
                x.Descricao.ToLower().Contains(termo) ||
                x.Cliente.Usuario.Nome.ToLower().Contains(termo) ||
                x.Cliente.Usuario.Email.ToLower().Contains(termo) ||
                x.Profissional.Usuario.Nome.ToLower().Contains(termo) ||
                x.Profissional.Usuario.Email.ToLower().Contains(termo) ||
                x.Profissional.NomeExibicao.ToLower().Contains(termo));
        }

        if (request.ClienteId.HasValue)
            query = query.Where(x => x.ClienteId == request.ClienteId.Value);

        if (request.ProfissionalId.HasValue)
            query = query.Where(x => x.ProfissionalId == request.ProfissionalId.Value);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

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
            .Select(x => new ServicoAdminListItemResponse
            {
                Id = x.Id,
                ClienteId = x.ClienteId,
                ProfissionalId = x.ProfissionalId,
                NomeCliente = x.Cliente.Usuario.Nome,
                NomeProfissional = x.Profissional.NomeExibicao,
                Titulo = x.Titulo,
                Status = x.Status,
                ValorCombinado = x.ValorCombinado,
                DataCriacao = x.DataCriacao,
                DataConclusao = x.DataConclusao
            })
            .ToListAsync(cancellationToken);

        return new PaginacaoResponse<ServicoAdminListItemResponse>
        {
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalRegistros = totalRegistros,
            TotalPaginas = totalRegistros == 0 ? 0 : (int)Math.Ceiling(totalRegistros / (double)tamanhoPagina),
            Itens = itens
        };
    }

    public async Task<ServicoAdminDetalheResponse?> ObterPorIdAsync(
        Guid servicoId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Servicos
            .AsNoTracking()
            .Include(x => x.Cliente).ThenInclude(x => x.Usuario)
            .Include(x => x.Profissional).ThenInclude(x => x.Usuario)
            .Include(x => x.Profissao)
            .Include(x => x.Especialidade)
            .Include(x => x.Cidade).ThenInclude(x => x.Estado)
            .Include(x => x.Bairro)
            .Where(x => x.Id == servicoId)
            .Select(x => new ServicoAdminDetalheResponse
            {
                Id = x.Id,
                ClienteId = x.ClienteId,
                ProfissionalId = x.ProfissionalId,
                ProfissaoId = x.ProfissaoId,
                EspecialidadeId = x.EspecialidadeId,
                CidadeId = x.CidadeId,
                BairroId = x.BairroId,
                NomeCliente = x.Cliente.Usuario.Nome,
                EmailCliente = x.Cliente.Usuario.Email,
                NomeProfissional = x.Profissional.NomeExibicao,
                EmailProfissional = x.Profissional.Usuario.Email,
                NomeProfissao = x.Profissao != null ? x.Profissao.Nome : null,
                NomeEspecialidade = x.Especialidade != null ? x.Especialidade.Nome : null,
                CidadeNome = x.Cidade.Nome,
                UF = x.Cidade.Estado.UF,
                BairroNome = x.Bairro != null ? x.Bairro.Nome : null,
                Titulo = x.Titulo,
                Descricao = x.Descricao,
                ValorCombinado = x.ValorCombinado,
                Status = x.Status,
                DataCriacao = x.DataCriacao,
                DataAceite = x.DataAceite,
                DataInicio = x.DataInicio,
                DataConclusao = x.DataConclusao,
                DataCancelamento = x.DataCancelamento
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
