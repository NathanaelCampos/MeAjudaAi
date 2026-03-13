using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.Interfaces.Admin;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Infrastructure.Services.Admin;

public class AdminProfissionalService : IAdminProfissionalService
{
    private readonly AppDbContext _context;
    private readonly IAdminAuditoriaService _adminAuditoriaService;

    public AdminProfissionalService(
        AppDbContext context,
        IAdminAuditoriaService adminAuditoriaService)
    {
        _context = context;
        _adminAuditoriaService = adminAuditoriaService;
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

    public async Task<ProfissionalAdminDashboardResponse?> ObterDashboardAsync(
        Guid profissionalId,
        CancellationToken cancellationToken = default)
    {
        var profissional = await ObterPorIdAsync(profissionalId, cancellationToken);

        if (profissional is null)
            return null;

        var usuarioId = profissional.UsuarioId;

        var totalNotificacoesAtivas = await _context.NotificacoesUsuarios
            .AsNoTracking()
            .Where(x => x.UsuarioId == usuarioId && x.Ativo)
            .CountAsync(cancellationToken);

        var notificacoesNaoLidas = await _context.NotificacoesUsuarios
            .AsNoTracking()
            .Where(x => x.UsuarioId == usuarioId && x.Ativo && x.DataLeitura == null)
            .CountAsync(cancellationToken);

        var notificacoesLidas = await _context.NotificacoesUsuarios
            .AsNoTracking()
            .Where(x => x.UsuarioId == usuarioId && x.Ativo && x.DataLeitura != null)
            .CountAsync(cancellationToken);

        var notificacoesArquivadas = await _context.NotificacoesUsuarios
            .AsNoTracking()
            .Where(x => x.UsuarioId == usuarioId && !x.Ativo)
            .CountAsync(cancellationToken);

        var emailsQuery = _context.EmailsNotificacoesOutbox
            .AsNoTracking()
            .Where(x => x.UsuarioId == usuarioId);

        var totalEmails = await emailsQuery.CountAsync(cancellationToken);
        var emailsPendentes = await emailsQuery.Where(x => x.Status == StatusEmailNotificacao.Pendente).CountAsync(cancellationToken);
        var emailsEnviados = await emailsQuery.Where(x => x.Status == StatusEmailNotificacao.Enviado).CountAsync(cancellationToken);
        var emailsFalhas = await emailsQuery.Where(x => x.Status == StatusEmailNotificacao.Falha).CountAsync(cancellationToken);
        var emailsCancelados = await emailsQuery.Where(x => x.Status == StatusEmailNotificacao.Cancelado).CountAsync(cancellationToken);

        var ultimoEmail = await emailsQuery
            .OrderByDescending(x => x.DataCriacao)
            .Select(x => new { x.Status, x.DataCriacao })
            .FirstOrDefaultAsync(cancellationToken);

        var servicosQuery = _context.Servicos
            .AsNoTracking()
            .Where(x => x.ProfissionalId == profissionalId && x.Ativo);

        var totalServicos = await servicosQuery.CountAsync(cancellationToken);
        var servicosSolicitados = await servicosQuery.Where(x => x.Status == StatusServico.Solicitado).CountAsync(cancellationToken);
        var servicosAceitos = await servicosQuery.Where(x => x.Status == StatusServico.Aceito).CountAsync(cancellationToken);
        var servicosEmExecucao = await servicosQuery.Where(x => x.Status == StatusServico.EmExecucao).CountAsync(cancellationToken);
        var servicosConcluidos = await servicosQuery.Where(x => x.Status == StatusServico.Concluido).CountAsync(cancellationToken);
        var servicosCancelados = await servicosQuery.Where(x => x.Status == StatusServico.Cancelado).CountAsync(cancellationToken);
        var ultimaDataCriacaoServico = await servicosQuery
            .OrderByDescending(x => x.DataCriacao)
            .Select(x => (DateTime?)x.DataCriacao)
            .FirstOrDefaultAsync(cancellationToken);

        var avaliacoesQuery = _context.Avaliacoes
            .AsNoTracking()
            .Where(x => x.ProfissionalId == profissionalId && x.Ativo);

        var totalAvaliacoes = await avaliacoesQuery.CountAsync(cancellationToken);
        var avaliacoesPendentes = await avaliacoesQuery.Where(x => x.StatusModeracaoComentario == StatusModeracaoComentario.Pendente).CountAsync(cancellationToken);
        var avaliacoesAprovadas = await avaliacoesQuery.Where(x => x.StatusModeracaoComentario == StatusModeracaoComentario.Aprovado).CountAsync(cancellationToken);
        var avaliacoesRejeitadas = await avaliacoesQuery.Where(x => x.StatusModeracaoComentario == StatusModeracaoComentario.Rejeitado).CountAsync(cancellationToken);
        var avaliacoesOcultas = await avaliacoesQuery.Where(x => x.StatusModeracaoComentario == StatusModeracaoComentario.Oculto).CountAsync(cancellationToken);
        var ultimaDataCriacaoAvaliacao = await avaliacoesQuery
            .OrderByDescending(x => x.DataCriacao)
            .Select(x => (DateTime?)x.DataCriacao)
            .FirstOrDefaultAsync(cancellationToken);

        var impulsionamentosQuery = _context.ImpulsionamentosProfissionais
            .AsNoTracking()
            .Where(x => x.ProfissionalId == profissionalId && x.Ativo);

        var totalImpulsionamentos = await impulsionamentosQuery.CountAsync(cancellationToken);
        var impulsionamentosPendentes = await impulsionamentosQuery.Where(x => x.Status == StatusImpulsionamento.PendentePagamento).CountAsync(cancellationToken);
        var impulsionamentosAtivos = await impulsionamentosQuery.Where(x => x.Status == StatusImpulsionamento.Ativo).CountAsync(cancellationToken);
        var impulsionamentosEncerrados = await impulsionamentosQuery.Where(x => x.Status == StatusImpulsionamento.Encerrado).CountAsync(cancellationToken);
        var impulsionamentosCancelados = await impulsionamentosQuery.Where(x => x.Status == StatusImpulsionamento.Cancelado).CountAsync(cancellationToken);
        var impulsionamentosExpirados = await impulsionamentosQuery.Where(x => x.Status == StatusImpulsionamento.Expirado).CountAsync(cancellationToken);
        var ultimoPeriodoImpulsionamento = await impulsionamentosQuery
            .OrderByDescending(x => x.DataCriacao)
            .Select(x => new { x.DataInicio, x.DataFim })
            .FirstOrDefaultAsync(cancellationToken);

        return new ProfissionalAdminDashboardResponse
        {
            Profissional = profissional,
            Notificacoes = new UsuarioAdminDashboardNotificacoesResponse
            {
                TotalAtivas = totalNotificacoesAtivas,
                NaoLidas = notificacoesNaoLidas,
                Lidas = notificacoesLidas,
                Arquivadas = notificacoesArquivadas
            },
            Emails = new UsuarioAdminDashboardEmailsResponse
            {
                Total = totalEmails,
                Pendentes = emailsPendentes,
                Enviados = emailsEnviados,
                Falhas = emailsFalhas,
                Cancelados = emailsCancelados,
                UltimoStatus = ultimoEmail?.Status,
                UltimaDataCriacao = ultimoEmail?.DataCriacao
            },
            Servicos = new ProfissionalAdminDashboardServicosResponse
            {
                Total = totalServicos,
                Solicitados = servicosSolicitados,
                Aceitos = servicosAceitos,
                EmExecucao = servicosEmExecucao,
                Concluidos = servicosConcluidos,
                Cancelados = servicosCancelados,
                UltimaDataCriacao = ultimaDataCriacaoServico
            },
            Avaliacoes = new ProfissionalAdminDashboardAvaliacoesResponse
            {
                Total = totalAvaliacoes,
                Pendentes = avaliacoesPendentes,
                Aprovadas = avaliacoesAprovadas,
                Rejeitadas = avaliacoesRejeitadas,
                Ocultas = avaliacoesOcultas,
                NotaMediaAtendimento = profissional.NotaMediaAtendimento,
                NotaMediaServico = profissional.NotaMediaServico,
                NotaMediaPreco = profissional.NotaMediaPreco,
                UltimaDataCriacao = ultimaDataCriacaoAvaliacao
            },
            Impulsionamentos = new ProfissionalAdminDashboardImpulsionamentosResponse
            {
                Total = totalImpulsionamentos,
                PendentesPagamento = impulsionamentosPendentes,
                Ativos = impulsionamentosAtivos,
                Encerrados = impulsionamentosEncerrados,
                Cancelados = impulsionamentosCancelados,
                Expirados = impulsionamentosExpirados,
                UltimaDataInicio = ultimoPeriodoImpulsionamento?.DataInicio,
                UltimaDataFim = ultimoPeriodoImpulsionamento?.DataFim
            }
        };
    }

    public async Task<ProfissionalAdminDetalheResponse> DefinirPerfilVerificadoAsync(
        Guid profissionalId,
        bool perfilVerificado,
        Guid? usuarioAdministradorId = null,
        CancellationToken cancellationToken = default)
    {
        var profissional = await _context.Profissionais
            .FirstOrDefaultAsync(x => x.Id == profissionalId, cancellationToken);

        if (profissional is null)
            throw new InvalidOperationException("Profissional não encontrado.");

        profissional.PerfilVerificado = perfilVerificado;
        profissional.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        if (usuarioAdministradorId.HasValue)
        {
            await _adminAuditoriaService.RegistrarAsync(
                usuarioAdministradorId.Value,
                "profissional",
                profissional.Id,
                perfilVerificado ? "verificar" : "desverificar",
                perfilVerificado ? "Administrador verificou o perfil profissional." : "Administrador removeu a verificação do perfil profissional.",
                $$"""
                {"profissionalId":"{{profissional.Id}}","perfilVerificado":{{perfilVerificado.ToString().ToLowerInvariant()}}}
                """,
                cancellationToken);
        }

        return (await ObterPorIdAsync(profissionalId, cancellationToken))!;
    }

    public async Task<ProfissionalAdminDetalheResponse> DefinirAtivoAsync(
        Guid profissionalId,
        bool ativo,
        Guid? usuarioAdministradorId = null,
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

        if (usuarioAdministradorId.HasValue)
        {
            await _adminAuditoriaService.RegistrarAsync(
                usuarioAdministradorId.Value,
                "profissional",
                profissional.Id,
                ativo ? "ativar" : "desativar",
                ativo ? "Administrador ativou o profissional." : "Administrador desativou o profissional.",
                $$"""
                {"profissionalId":"{{profissional.Id}}","ativo":{{ativo.ToString().ToLowerInvariant()}}}
                """,
                cancellationToken);
        }

        return (await ObterPorIdAsync(profissionalId, cancellationToken))!;
    }
}
