using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.Interfaces.Admin;
using MeAjudaAi.Domain.Enums;
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

    public async Task<ServicoAdminDashboardResponse?> ObterDashboardAsync(
        Guid servicoId,
        CancellationToken cancellationToken = default)
    {
        var servico = await ObterPorIdAsync(servicoId, cancellationToken);

        if (servico is null)
            return null;

        var notificacoesQuery = _context.NotificacoesUsuarios
            .AsNoTracking()
            .Where(x => x.ReferenciaId == servicoId);

        var totalNotificacoesAtivas = await notificacoesQuery
            .Where(x => x.Ativo)
            .CountAsync(cancellationToken);

        var notificacoesNaoLidas = await notificacoesQuery
            .Where(x => x.Ativo && x.DataLeitura == null)
            .CountAsync(cancellationToken);

        var notificacoesLidas = await notificacoesQuery
            .Where(x => x.Ativo && x.DataLeitura != null)
            .CountAsync(cancellationToken);

        var notificacoesArquivadas = await notificacoesQuery
            .Where(x => !x.Ativo)
            .CountAsync(cancellationToken);

        var ultimaDataCriacaoNotificacao = await notificacoesQuery
            .OrderByDescending(x => x.DataCriacao)
            .Select(x => (DateTime?)x.DataCriacao)
            .FirstOrDefaultAsync(cancellationToken);

        var emailsQuery = _context.EmailsNotificacoesOutbox
            .AsNoTracking()
            .Where(x => x.ReferenciaId == servicoId);

        var totalEmails = await emailsQuery.CountAsync(cancellationToken);
        var emailsPendentes = await emailsQuery.Where(x => x.Status == StatusEmailNotificacao.Pendente).CountAsync(cancellationToken);
        var emailsEnviados = await emailsQuery.Where(x => x.Status == StatusEmailNotificacao.Enviado).CountAsync(cancellationToken);
        var emailsFalhas = await emailsQuery.Where(x => x.Status == StatusEmailNotificacao.Falha).CountAsync(cancellationToken);
        var emailsCancelados = await emailsQuery.Where(x => x.Status == StatusEmailNotificacao.Cancelado).CountAsync(cancellationToken);

        var ultimoEmail = await emailsQuery
            .OrderByDescending(x => x.DataCriacao)
            .Select(x => new { x.Status, x.DataCriacao })
            .FirstOrDefaultAsync(cancellationToken);

        var avaliacao = await _context.Avaliacoes
            .AsNoTracking()
            .Include(x => x.Cliente).ThenInclude(x => x.Usuario)
            .Where(x => x.ServicoId == servicoId && x.Ativo)
            .Select(x => new ServicoAdminDashboardAvaliacaoResponse
            {
                Id = x.Id,
                ClienteId = x.ClienteId,
                ProfissionalId = x.ProfissionalId,
                NomeCliente = x.Cliente.Usuario.Nome,
                NotaAtendimento = x.NotaAtendimento,
                NotaServico = x.NotaServico,
                NotaPreco = x.NotaPreco,
                Comentario = x.Comentario,
                StatusModeracaoComentario = x.StatusModeracaoComentario,
                DataCriacao = x.DataCriacao
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new ServicoAdminDashboardResponse
        {
            Servico = servico,
            Notificacoes = new ServicoAdminDashboardNotificacoesResponse
            {
                TotalAtivas = totalNotificacoesAtivas,
                NaoLidas = notificacoesNaoLidas,
                Lidas = notificacoesLidas,
                Arquivadas = notificacoesArquivadas,
                UltimaDataCriacao = ultimaDataCriacaoNotificacao
            },
            Emails = new ServicoAdminDashboardEmailsResponse
            {
                Total = totalEmails,
                Pendentes = emailsPendentes,
                Enviados = emailsEnviados,
                Falhas = emailsFalhas,
                Cancelados = emailsCancelados,
                UltimoStatus = ultimoEmail?.Status,
                UltimaDataCriacao = ultimoEmail?.DataCriacao
            },
            Avaliacao = avaliacao
        };
    }
}
