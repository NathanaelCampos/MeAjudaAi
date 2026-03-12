using MeAjudaAi.Application.DTOs.Servicos;
using MeAjudaAi.Application.Interfaces.Notificacoes;
using MeAjudaAi.Application.Interfaces.Servicos;
using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Infrastructure.Services.Servicos;

public class ServicoService : IServicoService
{
    private readonly AppDbContext _context;
    private readonly INotificacaoService _notificacaoService;

    public ServicoService(
        AppDbContext context,
        INotificacaoService notificacaoService)
    {
        _context = context;
        _notificacaoService = notificacaoService;
    }

    public async Task<ServicoResponse> CriarAsync(
        Guid usuarioId,
        CriarServicoRequest request,
        CancellationToken cancellationToken = default)
    {
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId, cancellationToken);

        if (cliente is null)
            throw new InvalidOperationException("Cliente não encontrado para o usuário autenticado.");

        var profissional = await _context.Profissionais
            .FirstOrDefaultAsync(x => x.Id == request.ProfissionalId && x.Ativo, cancellationToken);

        if (profissional is null)
            throw new InvalidOperationException("Profissional não encontrado.");

        var cidadeExiste = await _context.Cidades
            .AnyAsync(x => x.Id == request.CidadeId && x.Ativo, cancellationToken);

        if (!cidadeExiste)
            throw new InvalidOperationException("Cidade não encontrada.");

        if (request.BairroId.HasValue)
        {
            var bairroExiste = await _context.Bairros
                .AnyAsync(x => x.Id == request.BairroId.Value && x.Ativo, cancellationToken);

            if (!bairroExiste)
                throw new InvalidOperationException("Bairro não encontrado.");
        }

        var servico = new Servico
        {
            ClienteId = cliente.Id,
            ProfissionalId = request.ProfissionalId,
            ProfissaoId = request.ProfissaoId,
            EspecialidadeId = request.EspecialidadeId,
            CidadeId = request.CidadeId,
            BairroId = request.BairroId,
            Titulo = request.Titulo.Trim(),
            Descricao = request.Descricao.Trim(),
            ValorCombinado = request.ValorCombinado,
            Status = StatusServico.Solicitado
        };

        _context.Servicos.Add(servico);
        await _context.SaveChangesAsync(cancellationToken);

        await _notificacaoService.CriarAsync(
            profissional.UsuarioId,
            TipoNotificacao.ServicoSolicitado,
            "Novo serviço solicitado",
            $"Você recebeu uma nova solicitação de serviço: {servico.Titulo}.",
            servico.Id,
            cancellationToken);

        return await ObterInternoAsync(servico.Id, cancellationToken)
            ?? throw new InvalidOperationException("Não foi possível carregar o serviço criado.");
    }

    public async Task<ServicoResponse?> ObterPorIdAsync(
        Guid usuarioId,
        Guid servicoId,
        CancellationToken cancellationToken = default)
    {
        var servico = await _context.Servicos
            .AsNoTracking()
            .Include(x => x.Cliente)
            .Include(x => x.Profissional)
            .Include(x => x.Profissao)
            .Include(x => x.Especialidade)
            .Include(x => x.Cidade)
                .ThenInclude(x => x.Estado)
            .Include(x => x.Bairro)
            .FirstOrDefaultAsync(x => x.Id == servicoId, cancellationToken);

        if (servico is null)
            return null;

        var cliente = await _context.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId, cancellationToken);

        var profissional = await _context.Profissionais
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId, cancellationToken);

        var usuarioPodeVer =
            (cliente is not null && servico.ClienteId == cliente.Id) ||
            (profissional is not null && servico.ProfissionalId == profissional.Id);

        if (!usuarioPodeVer)
            throw new InvalidOperationException("Você não pode acessar este serviço.");

        return Mapear(servico);
    }

    public async Task<ServicoResponse?> AceitarAsync(
        Guid usuarioId,
        Guid servicoId,
        CancellationToken cancellationToken = default)
    {
        var profissional = await _context.Profissionais
            .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId, cancellationToken);

        if (profissional is null)
            throw new InvalidOperationException("Profissional não encontrado para o usuário autenticado.");

        var servico = await _context.Servicos
            .FirstOrDefaultAsync(x => x.Id == servicoId, cancellationToken);

        if (servico is null)
            return null;

        if (servico.ProfissionalId != profissional.Id)
            throw new InvalidOperationException("Você não pode aceitar este serviço.");

        if (servico.Status != StatusServico.Solicitado)
            throw new InvalidOperationException("Somente serviços solicitados podem ser aceitos.");

        servico.Status = StatusServico.Aceito;
        servico.DataAceite = DateTime.UtcNow;
        servico.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var usuarioClienteId = await _context.Clientes
            .Where(x => x.Id == servico.ClienteId)
            .Select(x => x.UsuarioId)
            .FirstAsync(cancellationToken);

        await _notificacaoService.CriarAsync(
            usuarioClienteId,
            TipoNotificacao.ServicoAceito,
            "Serviço aceito",
            $"Seu serviço \"{servico.Titulo}\" foi aceito pelo profissional.",
            servico.Id,
            cancellationToken);

        return await ObterInternoAsync(servico.Id, cancellationToken);
    }

    public async Task<ServicoResponse?> IniciarAsync(
        Guid usuarioId,
        Guid servicoId,
        CancellationToken cancellationToken = default)
    {
        var profissional = await _context.Profissionais
            .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId, cancellationToken);

        if (profissional is null)
            throw new InvalidOperationException("Profissional não encontrado para o usuário autenticado.");

        var servico = await _context.Servicos
            .FirstOrDefaultAsync(x => x.Id == servicoId, cancellationToken);

        if (servico is null)
            return null;

        if (servico.ProfissionalId != profissional.Id)
            throw new InvalidOperationException("Você não pode iniciar este serviço.");

        if (servico.Status != StatusServico.Aceito)
            throw new InvalidOperationException("Somente serviços aceitos podem ser iniciados.");

        servico.Status = StatusServico.EmExecucao;
        servico.DataInicio = DateTime.UtcNow;
        servico.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await ObterInternoAsync(servico.Id, cancellationToken);
    }

    public async Task<ServicoResponse?> ConcluirAsync(
        Guid usuarioId,
        Guid servicoId,
        CancellationToken cancellationToken = default)
    {
        var profissional = await _context.Profissionais
            .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId, cancellationToken);

        if (profissional is null)
            throw new InvalidOperationException("Profissional não encontrado para o usuário autenticado.");

        var servico = await _context.Servicos
            .FirstOrDefaultAsync(x => x.Id == servicoId, cancellationToken);

        if (servico is null)
            return null;

        if (servico.ProfissionalId != profissional.Id)
            throw new InvalidOperationException("Você não pode concluir este serviço.");

        if (servico.Status != StatusServico.Aceito && servico.Status != StatusServico.EmExecucao)
            throw new InvalidOperationException("Somente serviços aceitos ou em execução podem ser concluídos.");

        servico.Status = StatusServico.Concluido;
        servico.DataConclusao = DateTime.UtcNow;
        servico.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var usuarioClienteId = await _context.Clientes
            .Where(x => x.Id == servico.ClienteId)
            .Select(x => x.UsuarioId)
            .FirstAsync(cancellationToken);

        await _notificacaoService.CriarAsync(
            usuarioClienteId,
            TipoNotificacao.ServicoConcluido,
            "Serviço concluído",
            $"O serviço \"{servico.Titulo}\" foi marcado como concluído.",
            servico.Id,
            cancellationToken);

        return await ObterInternoAsync(servico.Id, cancellationToken);
    }

    public async Task<ServicoResponse?> CancelarAsync(
        Guid usuarioId,
        Guid servicoId,
        CancellationToken cancellationToken = default)
    {
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId, cancellationToken);

        var profissional = await _context.Profissionais
            .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId, cancellationToken);

        if (cliente is null && profissional is null)
            throw new InvalidOperationException("Usuário autenticado não possui perfil válido para esta ação.");

        var servico = await _context.Servicos
            .FirstOrDefaultAsync(x => x.Id == servicoId, cancellationToken);

        if (servico is null)
            return null;

        var podeCancelar =
            (cliente is not null && servico.ClienteId == cliente.Id) ||
            (profissional is not null && servico.ProfissionalId == profissional.Id);

        if (!podeCancelar)
            throw new InvalidOperationException("Você não pode cancelar este serviço.");

        if (servico.Status == StatusServico.Concluido)
            throw new InvalidOperationException("Serviços concluídos não podem ser cancelados.");

        if (servico.Status == StatusServico.Cancelado)
            throw new InvalidOperationException("Este serviço já está cancelado.");

        servico.Status = StatusServico.Cancelado;
        servico.DataCancelamento = DateTime.UtcNow;
        servico.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await ObterInternoAsync(servico.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<ServicoResponse>> ListarMeusServicosClienteAsync(
        Guid usuarioId,
        ListarServicosRequest request,
        CancellationToken cancellationToken = default)
    {
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId, cancellationToken);

        if (cliente is null)
            throw new InvalidOperationException("Cliente não encontrado para o usuário autenticado.");

        var query = _context.Servicos
            .AsNoTracking()
            .Include(x => x.Cliente)
            .Include(x => x.Profissional)
            .Include(x => x.Profissao)
            .Include(x => x.Especialidade)
            .Include(x => x.Cidade)
                .ThenInclude(x => x.Estado)
            .Include(x => x.Bairro)
            .Where(x => x.ClienteId == cliente.Id);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        return await query
            .OrderByDescending(x => x.DataCriacao)
            .Select(x => Mapear(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ServicoResponse>> ListarMeusServicosProfissionalAsync(
        Guid usuarioId,
        ListarServicosRequest request,
        CancellationToken cancellationToken = default)
    {
        var profissional = await _context.Profissionais
            .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId, cancellationToken);

        if (profissional is null)
            throw new InvalidOperationException("Profissional não encontrado para o usuário autenticado.");

        var query = _context.Servicos
            .AsNoTracking()
            .Include(x => x.Cliente)
            .Include(x => x.Profissional)
            .Include(x => x.Profissao)
            .Include(x => x.Especialidade)
            .Include(x => x.Cidade)
                .ThenInclude(x => x.Estado)
            .Include(x => x.Bairro)
            .Where(x => x.ProfissionalId == profissional.Id);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        return await query
            .OrderByDescending(x => x.DataCriacao)
            .Select(x => Mapear(x))
            .ToListAsync(cancellationToken);
    }

    private async Task<ServicoResponse?> ObterInternoAsync(
        Guid servicoId,
        CancellationToken cancellationToken = default)
    {
        var servico = await _context.Servicos
            .AsNoTracking()
            .Include(x => x.Cliente)
            .Include(x => x.Profissional)
            .Include(x => x.Profissao)
            .Include(x => x.Especialidade)
            .Include(x => x.Cidade)
                .ThenInclude(x => x.Estado)
            .Include(x => x.Bairro)
            .FirstOrDefaultAsync(x => x.Id == servicoId, cancellationToken);

        return servico is null ? null : Mapear(servico);
    }

    private static ServicoResponse Mapear(Servico servico)
    {
        return new ServicoResponse
        {
            Id = servico.Id,
            ClienteId = servico.ClienteId,
            ProfissionalId = servico.ProfissionalId,
            NomeCliente = servico.Cliente?.NomeExibicao ?? string.Empty,
            NomeProfissional = servico.Profissional?.NomeExibicao ?? string.Empty,
            ProfissaoId = servico.ProfissaoId,
            NomeProfissao = servico.Profissao?.Nome,
            EspecialidadeId = servico.EspecialidadeId,
            NomeEspecialidade = servico.Especialidade?.Nome,
            CidadeId = servico.CidadeId,
            CidadeNome = servico.Cidade?.Nome ?? string.Empty,
            UF = servico.Cidade?.Estado?.UF ?? string.Empty,
            BairroId = servico.BairroId,
            BairroNome = servico.Bairro?.Nome,
            Titulo = servico.Titulo,
            Descricao = servico.Descricao,
            ValorCombinado = servico.ValorCombinado,
            Status = servico.Status,
            DataCriacao = servico.DataCriacao,
            DataAceite = servico.DataAceite,
            DataInicio = servico.DataInicio,
            DataConclusao = servico.DataConclusao,
            DataCancelamento = servico.DataCancelamento
        };
    }
}
