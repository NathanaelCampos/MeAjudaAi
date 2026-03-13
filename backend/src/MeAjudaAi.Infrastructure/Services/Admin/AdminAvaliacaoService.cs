using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.Interfaces.Admin;
using MeAjudaAi.Domain.Enums;
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

    public async Task<AvaliacaoAdminDashboardResponse?> ObterDashboardAsync(
        Guid avaliacaoId,
        CancellationToken cancellationToken = default)
    {
        var avaliacao = await ObterPorIdAsync(avaliacaoId, cancellationToken);
        if (avaliacao is null)
            return null;

        var servico = await _context.Servicos
            .AsNoTracking()
            .Include(x => x.Cliente).ThenInclude(x => x.Usuario)
            .Include(x => x.Profissional).ThenInclude(x => x.Usuario)
            .Include(x => x.Profissao)
            .Include(x => x.Especialidade)
            .Where(x => x.Id == avaliacao.ServicoId)
            .Select(x => new AvaliacaoAdminDashboardServicoResponse
            {
                Id = x.Id,
                Titulo = x.Titulo,
                ClienteId = x.ClienteId,
                ProfissionalId = x.ProfissionalId,
                NomeCliente = x.Cliente.Usuario.Nome,
                NomeProfissional = x.Profissional.NomeExibicao,
                NomeProfissao = x.Profissao != null ? x.Profissao.Nome : null,
                NomeEspecialidade = x.Especialidade != null ? x.Especialidade.Nome : null
            })
            .FirstAsync(cancellationToken);

        var notificacoesQuery = _context.NotificacoesUsuarios
            .AsNoTracking()
            .Where(x => x.ReferenciaId == avaliacaoId);

        var totalNotificacoes = await notificacoesQuery.CountAsync(cancellationToken);
        var lidas = await notificacoesQuery.CountAsync(x => x.DataLeitura != null && x.Ativo, cancellationToken);
        var naoLidas = await notificacoesQuery.CountAsync(x => x.DataLeitura == null && x.Ativo, cancellationToken);
        var arquivadas = await notificacoesQuery.CountAsync(x => !x.Ativo, cancellationToken);
        var ultimaDataNotificacao = await notificacoesQuery.MaxAsync(x => (DateTime?)x.DataCriacao, cancellationToken);

        var emailsQuery = _context.EmailsNotificacoesOutbox
            .AsNoTracking()
            .Where(x => x.ReferenciaId == avaliacaoId);

        var totalEmails = await emailsQuery.CountAsync(cancellationToken);
        var pendentes = await emailsQuery.CountAsync(x => x.Status == StatusEmailNotificacao.Pendente, cancellationToken);
        var enviados = await emailsQuery.CountAsync(x => x.Status == StatusEmailNotificacao.Enviado, cancellationToken);
        var falhas = await emailsQuery.CountAsync(x => x.Status == StatusEmailNotificacao.Falha, cancellationToken);
        var cancelados = await emailsQuery.CountAsync(x => x.Status == StatusEmailNotificacao.Cancelado, cancellationToken);
        var ultimaDataEmail = await emailsQuery.MaxAsync(x => (DateTime?)x.DataCriacao, cancellationToken);

        return new AvaliacaoAdminDashboardResponse
        {
            Avaliacao = avaliacao,
            Servico = servico,
            Notificacoes = new AvaliacaoAdminDashboardNotificacoesResponse
            {
                Total = totalNotificacoes,
                Lidas = lidas,
                NaoLidas = naoLidas,
                Arquivadas = arquivadas,
                UltimaDataCriacao = ultimaDataNotificacao
            },
            Emails = new AvaliacaoAdminDashboardEmailsResponse
            {
                Total = totalEmails,
                Pendentes = pendentes,
                Enviados = enviados,
                Falhas = falhas,
                Cancelados = cancelados,
                UltimaDataCriacao = ultimaDataEmail
            }
        };
    }
}
