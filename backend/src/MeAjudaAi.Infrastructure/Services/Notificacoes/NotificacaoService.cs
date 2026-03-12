using MeAjudaAi.Application.DTOs.Notificacoes;
using MeAjudaAi.Application.Interfaces.Notificacoes;
using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Infrastructure.Services.Notificacoes;

public class NotificacaoService : INotificacaoService
{
    private readonly AppDbContext _context;

    public NotificacaoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task CriarAsync(
        Guid usuarioId,
        TipoNotificacao tipo,
        string titulo,
        string mensagem,
        Guid? referenciaId = null,
        CancellationToken cancellationToken = default)
    {
        var notificacao = new NotificacaoUsuario
        {
            UsuarioId = usuarioId,
            Tipo = tipo,
            Titulo = titulo.Trim(),
            Mensagem = mensagem.Trim(),
            ReferenciaId = referenciaId
        };

        _context.Set<NotificacaoUsuario>().Add(notificacao);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<NotificacaoResponse>> ListarMinhasAsync(
        Guid usuarioId,
        bool somenteNaoLidas = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<NotificacaoUsuario>()
            .AsNoTracking()
            .Where(x => x.UsuarioId == usuarioId && x.Ativo);

        if (somenteNaoLidas)
            query = query.Where(x => x.DataLeitura == null);

        return await query
            .OrderByDescending(x => x.DataCriacao)
            .Take(100)
            .Select(x => new NotificacaoResponse
            {
                Id = x.Id,
                Tipo = x.Tipo,
                Titulo = x.Titulo,
                Mensagem = x.Mensagem,
                ReferenciaId = x.ReferenciaId,
                Lida = x.DataLeitura.HasValue,
                DataCriacao = x.DataCriacao,
                DataLeitura = x.DataLeitura
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<QuantidadeNotificacoesNaoLidasResponse> ObterQuantidadeNaoLidasAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        var quantidade = await _context.Set<NotificacaoUsuario>()
            .AsNoTracking()
            .CountAsync(x => x.UsuarioId == usuarioId && x.Ativo && x.DataLeitura == null, cancellationToken);

        return new QuantidadeNotificacoesNaoLidasResponse
        {
            Quantidade = quantidade
        };
    }

    public async Task<NotificacaoResponse?> MarcarComoLidaAsync(
        Guid usuarioId,
        Guid notificacaoId,
        CancellationToken cancellationToken = default)
    {
        var notificacao = await _context.Set<NotificacaoUsuario>()
            .FirstOrDefaultAsync(
                x => x.Id == notificacaoId && x.UsuarioId == usuarioId && x.Ativo,
                cancellationToken);

        if (notificacao is null)
            return null;

        if (!notificacao.DataLeitura.HasValue)
        {
            notificacao.DataLeitura = DateTime.UtcNow;
            notificacao.DataAtualizacao = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Mapear(notificacao);
    }

    public async Task<int> MarcarTodasComoLidasAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        var notificacoes = await _context.Set<NotificacaoUsuario>()
            .Where(x => x.UsuarioId == usuarioId && x.Ativo && x.DataLeitura == null)
            .ToListAsync(cancellationToken);

        if (notificacoes.Count == 0)
            return 0;

        var agora = DateTime.UtcNow;

        foreach (var notificacao in notificacoes)
        {
            notificacao.DataLeitura = agora;
            notificacao.DataAtualizacao = agora;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return notificacoes.Count;
    }

    private static NotificacaoResponse Mapear(NotificacaoUsuario notificacao)
    {
        return new NotificacaoResponse
        {
            Id = notificacao.Id,
            Tipo = notificacao.Tipo,
            Titulo = notificacao.Titulo,
            Mensagem = notificacao.Mensagem,
            ReferenciaId = notificacao.ReferenciaId,
            Lida = notificacao.DataLeitura.HasValue,
            DataCriacao = notificacao.DataCriacao,
            DataLeitura = notificacao.DataLeitura
        };
    }
}
