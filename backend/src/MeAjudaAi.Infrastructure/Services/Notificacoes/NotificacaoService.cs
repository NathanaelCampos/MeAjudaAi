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
        if (!await PodeReceberNotificacaoInternaAsync(usuarioId, tipo, cancellationToken))
            return;

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

    public async Task<IReadOnlyList<PreferenciaNotificacaoResponse>> ListarPreferenciasAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        var preferencias = await _context.Set<PreferenciaNotificacaoUsuario>()
            .AsNoTracking()
            .Where(x => x.UsuarioId == usuarioId && x.Ativo)
            .ToListAsync(cancellationToken);

        return ListarTiposSuportados()
            .Select(tipo =>
            {
                var preferencia = preferencias.FirstOrDefault(x => x.Tipo == tipo);

                return new PreferenciaNotificacaoResponse
                {
                    Tipo = tipo,
                    AtivoInterno = preferencia?.AtivoInterno ?? true
                };
            })
            .ToArray();
    }

    public async Task<IReadOnlyList<PreferenciaNotificacaoResponse>> AtualizarPreferenciasAsync(
        Guid usuarioId,
        IReadOnlyList<PreferenciaNotificacaoItemRequest> preferencias,
        CancellationToken cancellationToken = default)
    {
        var tipos = preferencias.Select(x => x.Tipo).Distinct().ToArray();

        var existentes = await _context.Set<PreferenciaNotificacaoUsuario>()
            .Where(x => x.UsuarioId == usuarioId && tipos.Contains(x.Tipo))
            .ToListAsync(cancellationToken);

        var agora = DateTime.UtcNow;

        foreach (var item in preferencias)
        {
            var preferencia = existentes.FirstOrDefault(x => x.Tipo == item.Tipo);

            if (preferencia is null)
            {
                _context.Set<PreferenciaNotificacaoUsuario>().Add(new PreferenciaNotificacaoUsuario
                {
                    UsuarioId = usuarioId,
                    Tipo = item.Tipo,
                    AtivoInterno = item.AtivoInterno
                });

                continue;
            }

            preferencia.AtivoInterno = item.AtivoInterno;
            preferencia.Ativo = true;
            preferencia.DataAtualizacao = agora;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return await ListarPreferenciasAsync(usuarioId, cancellationToken);
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

    private async Task<bool> PodeReceberNotificacaoInternaAsync(
        Guid usuarioId,
        TipoNotificacao tipo,
        CancellationToken cancellationToken)
    {
        var preferencia = await _context.Set<PreferenciaNotificacaoUsuario>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.UsuarioId == usuarioId && x.Tipo == tipo && x.Ativo,
                cancellationToken);

        return preferencia?.AtivoInterno ?? true;
    }

    private static TipoNotificacao[] ListarTiposSuportados()
    {
        return Enum.GetValues<TipoNotificacao>()
            .OrderBy(x => (int)x)
            .ToArray();
    }
}
