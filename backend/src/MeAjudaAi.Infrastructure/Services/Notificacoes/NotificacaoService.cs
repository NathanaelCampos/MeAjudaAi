using MeAjudaAi.Application.DTOs.Notificacoes;
using MeAjudaAi.Application.Interfaces.Notificacoes;
using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Infrastructure.Configurations;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Infrastructure.Services.Notificacoes;

public class NotificacaoService : INotificacaoService
{
    private readonly AppDbContext _context;
    private readonly IEmailNotificacaoSender _emailNotificacaoSender;
    private readonly EmailNotificacaoOptions _emailOptions;

    public NotificacaoService(
        AppDbContext context,
        IEmailNotificacaoSender emailNotificacaoSender,
        IOptions<EmailNotificacaoOptions> emailOptions)
    {
        _context = context;
        _emailNotificacaoSender = emailNotificacaoSender;
        _emailOptions = emailOptions.Value;
    }

    public async Task CriarAsync(
        Guid usuarioId,
        TipoNotificacao tipo,
        string titulo,
        string mensagem,
        Guid? referenciaId = null,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _context.Set<Usuario>()
            .AsNoTracking()
            .Where(x => x.Id == usuarioId && x.Ativo)
            .Select(x => new { x.Id, x.Email })
            .FirstOrDefaultAsync(cancellationToken);

        if (usuario is null)
            return;

        var deveReceberInterna = await PodeReceberNotificacaoInternaAsync(usuarioId, tipo, cancellationToken);
        var deveReceberEmail = await PodeReceberNotificacaoEmailAsync(usuarioId, tipo, cancellationToken);

        if (!deveReceberInterna && !deveReceberEmail)
            return;

        if (deveReceberInterna)
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
        }

        if (deveReceberEmail)
        {
            _context.Set<EmailNotificacaoOutbox>().Add(new EmailNotificacaoOutbox
            {
                UsuarioId = usuarioId,
                TipoNotificacao = tipo,
                EmailDestino = usuario.Email,
                Assunto = titulo.Trim(),
                Corpo = mensagem.Trim(),
                ReferenciaId = referenciaId,
                ProximaTentativaEm = DateTime.UtcNow
            });
        }

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
                    AtivoInterno = preferencia?.AtivoInterno ?? true,
                    AtivoEmail = preferencia?.AtivoEmail ?? false
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
                    AtivoInterno = item.AtivoInterno,
                    AtivoEmail = item.AtivoEmail
                });

                continue;
            }

            preferencia.AtivoInterno = item.AtivoInterno;
            preferencia.AtivoEmail = item.AtivoEmail;
            preferencia.Ativo = true;
            preferencia.DataAtualizacao = agora;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return await ListarPreferenciasAsync(usuarioId, cancellationToken);
    }

    public async Task<IReadOnlyList<EmailNotificacaoOutboxResponse>> ListarEmailsOutboxAsync(
        StatusEmailNotificacao? status = null,
        Guid? usuarioId = null,
        TipoNotificacao? tipoNotificacao = null,
        string? emailDestino = null,
        DateTime? dataCriacaoInicial = null,
        DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<EmailNotificacaoOutbox>()
            .AsNoTracking()
            .Where(x => x.Ativo);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        if (usuarioId.HasValue)
            query = query.Where(x => x.UsuarioId == usuarioId.Value);

        if (tipoNotificacao.HasValue)
            query = query.Where(x => x.TipoNotificacao == tipoNotificacao.Value);

        if (!string.IsNullOrWhiteSpace(emailDestino))
        {
            var emailNormalizado = emailDestino.Trim().ToLowerInvariant();
            query = query.Where(x => x.EmailDestino.ToLower().Contains(emailNormalizado));
        }

        if (dataCriacaoInicial.HasValue)
            query = query.Where(x => x.DataCriacao >= dataCriacaoInicial.Value);

        if (dataCriacaoFinal.HasValue)
            query = query.Where(x => x.DataCriacao <= dataCriacaoFinal.Value);

        return await query
            .OrderByDescending(x => x.DataCriacao)
            .Take(100)
            .Select(x => new EmailNotificacaoOutboxResponse
            {
                Id = x.Id,
                UsuarioId = x.UsuarioId,
                TipoNotificacao = x.TipoNotificacao,
                EmailDestino = x.EmailDestino,
                Assunto = x.Assunto,
                Corpo = x.Corpo,
                ReferenciaId = x.ReferenciaId,
                Status = x.Status,
                TentativasProcessamento = x.TentativasProcessamento,
                ProximaTentativaEm = x.ProximaTentativaEm,
                DataCriacao = x.DataCriacao,
                DataProcessamento = x.DataProcessamento,
                UltimaMensagemErro = x.UltimaMensagemErro
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<int> ReprocessarEmailsOutboxAsync(
        CancellationToken cancellationToken = default)
    {
        var emails = await _context.Set<EmailNotificacaoOutbox>()
            .Where(x =>
                x.Ativo &&
                (x.Status == StatusEmailNotificacao.Pendente || x.Status == StatusEmailNotificacao.Falha) &&
                (x.ProximaTentativaEm == null || x.ProximaTentativaEm <= DateTime.UtcNow))
            .OrderBy(x => x.DataCriacao)
            .Take(100)
            .ToListAsync(cancellationToken);

        if (emails.Count == 0)
            return 0;

        var agora = DateTime.UtcNow;

        foreach (var email in emails)
        {
            email.TentativasProcessamento++;

            try
            {
                await _emailNotificacaoSender.EnviarAsync(email, cancellationToken);
                email.Status = StatusEmailNotificacao.Enviado;
                email.DataProcessamento = agora;
                email.ProximaTentativaEm = null;
                email.UltimaMensagemErro = string.Empty;
                email.DataAtualizacao = agora;
            }
            catch (Exception ex)
            {
                email.DataProcessamento = agora;
                AtualizarFalha(email, ex.Message, agora);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return emails.Count;
    }

    public async Task<EmailNotificacaoMetricasResponse> ObterMetricasEmailsOutboxAsync(
        CancellationToken cancellationToken = default)
    {
        var itens = await _context.Set<EmailNotificacaoOutbox>()
            .AsNoTracking()
            .Where(x => x.Ativo)
            .GroupBy(x => x.Status)
            .Select(x => new EmailNotificacaoMetricaItemResponse
            {
                Status = x.Key,
                Quantidade = x.Count()
            })
            .OrderBy(x => x.Status)
            .ToListAsync(cancellationToken);

        return new EmailNotificacaoMetricasResponse
        {
            Itens = itens
        };
    }

    private void AtualizarFalha(EmailNotificacaoOutbox email, string mensagemErro, DateTime agora)
    {
        if (email.TentativasProcessamento >= Math.Max(1, _emailOptions.MaxTentativas))
        {
            email.Status = StatusEmailNotificacao.Cancelado;
            email.ProximaTentativaEm = null;
        }
        else
        {
            email.Status = StatusEmailNotificacao.Falha;
            email.ProximaTentativaEm = agora.AddSeconds(Math.Max(5, _emailOptions.AtrasoBaseSegundos) * email.TentativasProcessamento);
        }

        email.UltimaMensagemErro = mensagemErro;
        email.DataAtualizacao = agora;
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

    private async Task<bool> PodeReceberNotificacaoEmailAsync(
        Guid usuarioId,
        TipoNotificacao tipo,
        CancellationToken cancellationToken)
    {
        var preferencia = await _context.Set<PreferenciaNotificacaoUsuario>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.UsuarioId == usuarioId && x.Tipo == tipo && x.Ativo,
                cancellationToken);

        return preferencia?.AtivoEmail ?? false;
    }

    private static TipoNotificacao[] ListarTiposSuportados()
    {
        return Enum.GetValues<TipoNotificacao>()
            .OrderBy(x => (int)x)
            .ToArray();
    }
}
