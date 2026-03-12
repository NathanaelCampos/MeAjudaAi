using MeAjudaAi.Application.DTOs.Avaliacoes;
using MeAjudaAi.Application.Interfaces.Avaliacoes;
using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Infrastructure.Services.Avaliacoes;

public class AvaliacaoService : IAvaliacaoService
{
    private readonly AppDbContext _context;

    public AvaliacaoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AvaliacaoResponse> CriarAsync(
        Guid usuarioId,
        CriarAvaliacaoRequest request,
        CancellationToken cancellationToken = default)
    {
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId, cancellationToken);

        if (cliente is null)
            throw new InvalidOperationException("Cliente não encontrado para o usuário autenticado.");

        var servico = await _context.Servicos
            .FirstOrDefaultAsync(x => x.Id == request.ServicoId && x.Ativo, cancellationToken);

        if (servico is null)
            throw new InvalidOperationException("Serviço não encontrado.");

        if (servico.ClienteId != cliente.Id)
            throw new InvalidOperationException("Você não pode avaliar este serviço.");

        if (servico.Status != StatusServico.Concluido)
            throw new InvalidOperationException("Somente serviços concluídos podem ser avaliados.");

        var avaliacaoExistente = await _context.Avaliacoes
            .AnyAsync(x => x.ServicoId == servico.Id && x.Ativo, cancellationToken);

        if (avaliacaoExistente)
            throw new InvalidOperationException("Este serviço já foi avaliado.");

        var comentario = request.Comentario?.Trim() ?? string.Empty;

        if (comentario.Length > 1000)
            throw new InvalidOperationException("O comentário deve ter no máximo 1000 caracteres.");

        var avaliacao = new Avaliacao
        {
            ServicoId = servico.Id,
            ClienteId = cliente.Id,
            ProfissionalId = servico.ProfissionalId,
            NotaAtendimento = request.NotaAtendimento,
            NotaServico = request.NotaServico,
            NotaPreco = request.NotaPreco,
            Comentario = comentario,
            StatusModeracaoComentario = StatusModeracaoComentario.Pendente
        };

        _context.Avaliacoes.Add(avaliacao);
        await _context.SaveChangesAsync(cancellationToken);

        await AtualizarMediasProfissionalAsync(servico.ProfissionalId, cancellationToken);

        return new AvaliacaoResponse
        {
            Id = avaliacao.Id,
            ClienteId = avaliacao.ClienteId,
            ProfissionalId = avaliacao.ProfissionalId,
            NomeCliente = cliente.NomeExibicao,
            NotaAtendimento = avaliacao.NotaAtendimento,
            NotaServico = avaliacao.NotaServico,
            NotaPreco = avaliacao.NotaPreco,
            Comentario = avaliacao.Comentario,
            StatusModeracaoComentario = avaliacao.StatusModeracaoComentario,
            DataCriacao = avaliacao.DataCriacao
        };
    }

    public async Task<IReadOnlyList<AvaliacaoResponse>> ListarPorProfissionalAsync(
        Guid profissionalId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Avaliacoes
            .AsNoTracking()
            .Where(x =>
                x.ProfissionalId == profissionalId &&
                x.Ativo &&
                x.StatusModeracaoComentario != StatusModeracaoComentario.Rejeitado &&
                x.StatusModeracaoComentario != StatusModeracaoComentario.Oculto)
            .OrderByDescending(x => x.DataCriacao)
            .Select(x => new AvaliacaoResponse
            {
                Id = x.Id,
                ClienteId = x.ClienteId,
                ProfissionalId = x.ProfissionalId,
                NomeCliente = x.Cliente.NomeExibicao,
                NotaAtendimento = x.NotaAtendimento,
                NotaServico = x.NotaServico,
                NotaPreco = x.NotaPreco,
                Comentario = x.Comentario,
                StatusModeracaoComentario = x.StatusModeracaoComentario,
                DataCriacao = x.DataCriacao
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AvaliacaoResponse>> ListarPendentesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Avaliacoes
            .AsNoTracking()
            .Where(x => x.Ativo && x.StatusModeracaoComentario == StatusModeracaoComentario.Pendente)
            .OrderBy(x => x.DataCriacao)
            .Select(x => new AvaliacaoResponse
            {
                Id = x.Id,
                ClienteId = x.ClienteId,
                ProfissionalId = x.ProfissionalId,
                NomeCliente = x.Cliente.NomeExibicao,
                NotaAtendimento = x.NotaAtendimento,
                NotaServico = x.NotaServico,
                NotaPreco = x.NotaPreco,
                Comentario = x.Comentario,
                StatusModeracaoComentario = x.StatusModeracaoComentario,
                DataCriacao = x.DataCriacao
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AvaliacaoResponse?> ModerarAsync(
        Guid avaliacaoId,
        ModerarAvaliacaoRequest request,
        CancellationToken cancellationToken = default)
    {
        var avaliacao = await _context.Avaliacoes
            .Include(x => x.Cliente)
            .FirstOrDefaultAsync(x => x.Id == avaliacaoId && x.Ativo, cancellationToken);

        if (avaliacao is null)
            return null;

        avaliacao.StatusModeracaoComentario = request.Acao switch
        {
            AcaoModeracaoAvaliacao.Aprovar => StatusModeracaoComentario.Aprovado,
            AcaoModeracaoAvaliacao.Rejeitar => StatusModeracaoComentario.Rejeitado,
            AcaoModeracaoAvaliacao.Ocultar => StatusModeracaoComentario.Oculto,
            _ => avaliacao.StatusModeracaoComentario
        };

        avaliacao.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        await AtualizarMediasProfissionalAsync(avaliacao.ProfissionalId, cancellationToken);

        return new AvaliacaoResponse
        {
            Id = avaliacao.Id,
            ClienteId = avaliacao.ClienteId,
            ProfissionalId = avaliacao.ProfissionalId,
            NomeCliente = avaliacao.Cliente.NomeExibicao,
            NotaAtendimento = avaliacao.NotaAtendimento,
            NotaServico = avaliacao.NotaServico,
            NotaPreco = avaliacao.NotaPreco,
            Comentario = avaliacao.Comentario,
            StatusModeracaoComentario = avaliacao.StatusModeracaoComentario,
            DataCriacao = avaliacao.DataCriacao
        };
    }

    private async Task AtualizarMediasProfissionalAsync(
        Guid profissionalId,
        CancellationToken cancellationToken = default)
    {
        var profissional = await _context.Profissionais
            .FirstOrDefaultAsync(x => x.Id == profissionalId, cancellationToken);

        if (profissional is null)
            return;

        var avaliacoes = await _context.Avaliacoes
            .Where(x =>
                x.ProfissionalId == profissionalId &&
                x.Ativo &&
                x.StatusModeracaoComentario != StatusModeracaoComentario.Rejeitado &&
                x.StatusModeracaoComentario != StatusModeracaoComentario.Oculto)
            .ToListAsync(cancellationToken);

        if (avaliacoes.Count == 0)
        {
            profissional.NotaMediaAtendimento = null;
            profissional.NotaMediaServico = null;
            profissional.NotaMediaPreco = null;
        }
        else
        {
            profissional.NotaMediaAtendimento = Math.Round(avaliacoes.Average(x => (decimal)x.NotaAtendimento), 2);
            profissional.NotaMediaServico = Math.Round(avaliacoes.Average(x => (decimal)x.NotaServico), 2);
            profissional.NotaMediaPreco = Math.Round(avaliacoes.Average(x => (decimal)x.NotaPreco), 2);
        }

        profissional.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}