using MeAjudaAi.Application.DTOs.Notificacoes;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.Interfaces.Notificacoes;
using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Infrastructure.Configurations;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text;

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

    public async Task<PaginacaoResponse<NotificacaoAdminResponse>> ListarNotificacoesAsync(
        BuscarNotificacoesRequest request,
        CancellationToken cancellationToken = default)
    {
        return await ListarNotificacoesPorAtividadeAsync(
            request,
            ativo: true,
            cancellationToken);
    }

    public async Task<PaginacaoResponse<NotificacaoAdminResponse>> ListarNotificacoesArquivadasAsync(
        BuscarNotificacoesRequest request,
        CancellationToken cancellationToken = default)
    {
        return await ListarNotificacoesPorAtividadeAsync(
            request,
            ativo: false,
            cancellationToken);
    }

    private async Task<PaginacaoResponse<NotificacaoAdminResponse>> ListarNotificacoesPorAtividadeAsync(
        BuscarNotificacoesRequest request,
        bool ativo,
        CancellationToken cancellationToken)
    {
        var pagina = request.Pagina <= 0 ? 1 : request.Pagina;
        var tamanhoPagina = request.TamanhoPagina <= 0 ? 20 : Math.Min(request.TamanhoPagina, 100);

        var query = _context.Set<NotificacaoUsuario>()
            .AsNoTracking()
            .Where(x => x.Ativo == ativo);

        if (request.UsuarioId.HasValue)
            query = query.Where(x => x.UsuarioId == request.UsuarioId.Value);

        if (request.TipoNotificacao.HasValue)
            query = query.Where(x => x.Tipo == request.TipoNotificacao.Value);

        if (request.Lida.HasValue)
            query = request.Lida.Value ? query.Where(x => x.DataLeitura != null) : query.Where(x => x.DataLeitura == null);

        if (request.DataCriacaoInicial.HasValue)
            query = query.Where(x => x.DataCriacao >= request.DataCriacaoInicial.Value);

        if (request.DataCriacaoFinal.HasValue)
            query = query.Where(x => x.DataCriacao <= request.DataCriacaoFinal.Value);

        var totalRegistros = await query.CountAsync(cancellationToken);

        var itens = await query
            .OrderByDescending(x => x.DataCriacao)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(x => new NotificacaoAdminResponse
            {
                Id = x.Id,
                UsuarioId = x.UsuarioId,
                NomeUsuario = x.Usuario.Nome,
                EmailUsuario = x.Usuario.Email,
                Tipo = x.Tipo,
                Titulo = x.Titulo,
                Mensagem = x.Mensagem,
                ReferenciaId = x.ReferenciaId,
                Lida = x.DataLeitura != null,
                DataCriacao = x.DataCriacao,
                DataLeitura = x.DataLeitura
            })
            .ToListAsync(cancellationToken);

        return new PaginacaoResponse<NotificacaoAdminResponse>
        {
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalRegistros = totalRegistros,
            TotalPaginas = totalRegistros == 0 ? 0 : (int)Math.Ceiling(totalRegistros / (double)tamanhoPagina),
            Itens = itens
        };
    }

    public async Task<string> ExportarNotificacoesCsvAsync(
        ExportarNotificacoesRequest request,
        CancellationToken cancellationToken = default)
    {
        return await ExportarNotificacoesCsvPorAtividadeAsync(
            request,
            ativo: true,
            cancellationToken);
    }

    public async Task<string> ExportarNotificacoesArquivadasCsvAsync(
        ExportarNotificacoesRequest request,
        CancellationToken cancellationToken = default)
    {
        return await ExportarNotificacoesCsvPorAtividadeAsync(
            request,
            ativo: false,
            cancellationToken);
    }

    private async Task<string> ExportarNotificacoesCsvPorAtividadeAsync(
        ExportarNotificacoesRequest request,
        bool ativo,
        CancellationToken cancellationToken)
    {
        var query = _context.Set<NotificacaoUsuario>()
            .AsNoTracking()
            .Where(x => x.Ativo == ativo);

        if (request.UsuarioId.HasValue)
            query = query.Where(x => x.UsuarioId == request.UsuarioId.Value);

        if (request.TipoNotificacao.HasValue)
            query = query.Where(x => x.Tipo == request.TipoNotificacao.Value);

        if (request.Lida.HasValue)
            query = request.Lida.Value ? query.Where(x => x.DataLeitura != null) : query.Where(x => x.DataLeitura == null);

        if (request.DataCriacaoInicial.HasValue)
            query = query.Where(x => x.DataCriacao >= request.DataCriacaoInicial.Value);

        if (request.DataCriacaoFinal.HasValue)
            query = query.Where(x => x.DataCriacao <= request.DataCriacaoFinal.Value);

        var notificacoes = await query
            .OrderByDescending(x => x.DataCriacao)
            .Take(Math.Min(request.Limite, 5000))
            .Select(x => new NotificacaoAdminResponse
            {
                Id = x.Id,
                UsuarioId = x.UsuarioId,
                NomeUsuario = x.Usuario.Nome,
                EmailUsuario = x.Usuario.Email,
                Tipo = x.Tipo,
                Titulo = x.Titulo,
                Mensagem = x.Mensagem,
                ReferenciaId = x.ReferenciaId,
                Lida = x.DataLeitura != null,
                DataCriacao = x.DataCriacao,
                DataLeitura = x.DataLeitura
            })
            .ToListAsync(cancellationToken);

        var csv = new StringBuilder();
        csv.AppendLine("Id,UsuarioId,NomeUsuario,EmailUsuario,Tipo,Titulo,Mensagem,ReferenciaId,Lida,DataCriacao,DataLeitura");

        foreach (var notificacao in notificacoes)
        {
            csv.AppendLine(string.Join(",",
                EscaparCsv(notificacao.Id),
                EscaparCsv(notificacao.UsuarioId),
                EscaparCsv(notificacao.NomeUsuario),
                EscaparCsv(notificacao.EmailUsuario),
                EscaparCsv(notificacao.Tipo),
                EscaparCsv(notificacao.Titulo),
                EscaparCsv(notificacao.Mensagem),
                EscaparCsv(notificacao.ReferenciaId),
                EscaparCsv(notificacao.Lida),
                EscaparCsv(notificacao.DataCriacao),
                EscaparCsv(notificacao.DataLeitura)));
        }

        return csv.ToString();
    }

    public async Task<NotificacaoAdminResponse?> ObterNotificacaoPorIdAsync(
        Guid notificacaoId,
        CancellationToken cancellationToken = default)
    {
        return await ObterNotificacaoPorIdEAtividadeAsync(
            notificacaoId,
            ativo: true,
            cancellationToken);
    }

    public async Task<NotificacaoAdminResponse?> ObterNotificacaoArquivadaPorIdAsync(
        Guid notificacaoId,
        CancellationToken cancellationToken = default)
    {
        return await ObterNotificacaoPorIdEAtividadeAsync(
            notificacaoId,
            ativo: false,
            cancellationToken);
    }

    private async Task<NotificacaoAdminResponse?> ObterNotificacaoPorIdEAtividadeAsync(
        Guid notificacaoId,
        bool ativo,
        CancellationToken cancellationToken)
    {
        return await _context.Set<NotificacaoUsuario>()
            .AsNoTracking()
            .Where(x => x.Ativo == ativo && x.Id == notificacaoId)
            .Select(x => new NotificacaoAdminResponse
            {
                Id = x.Id,
                UsuarioId = x.UsuarioId,
                NomeUsuario = x.Usuario.Nome,
                EmailUsuario = x.Usuario.Email,
                Tipo = x.Tipo,
                Titulo = x.Titulo,
                Mensagem = x.Mensagem,
                ReferenciaId = x.ReferenciaId,
                Lida = x.DataLeitura != null,
                DataCriacao = x.DataCriacao,
                DataLeitura = x.DataLeitura
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> MarcarNotificacoesComoLidasEmLoteAsync(
        MarcarNotificacoesComoLidasEmLoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<NotificacaoUsuario>()
            .Where(x => x.Ativo && x.DataLeitura == null);

        if (request.UsuarioId.HasValue)
            query = query.Where(x => x.UsuarioId == request.UsuarioId.Value);

        if (request.TipoNotificacao.HasValue)
            query = query.Where(x => x.Tipo == request.TipoNotificacao.Value);

        if (request.DataCriacaoInicial.HasValue)
            query = query.Where(x => x.DataCriacao >= request.DataCriacaoInicial.Value);

        if (request.DataCriacaoFinal.HasValue)
            query = query.Where(x => x.DataCriacao <= request.DataCriacaoFinal.Value);

        var notificacoes = await query
            .OrderByDescending(x => x.DataCriacao)
            .Take(request.Limite)
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

    public async Task<int> ArquivarNotificacoesEmLoteAsync(
        ArquivarNotificacoesEmLoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var notificacoes = await AplicarFiltrosAtividade(
                _context.Set<NotificacaoUsuario>(),
                ativo: true,
                request)
            .OrderByDescending(x => x.DataCriacao)
            .Take(request.Limite)
            .ToListAsync(cancellationToken);

        if (notificacoes.Count == 0)
            return 0;

        var agora = DateTime.UtcNow;

        foreach (var notificacao in notificacoes)
        {
            notificacao.Ativo = false;
            notificacao.DataAtualizacao = agora;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return notificacoes.Count;
    }

    public async Task<int> RestaurarNotificacoesEmLoteAsync(
        ArquivarNotificacoesEmLoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var notificacoes = await AplicarFiltrosAtividade(
                _context.Set<NotificacaoUsuario>(),
                ativo: false,
                request)
            .OrderByDescending(x => x.DataCriacao)
            .Take(request.Limite)
            .ToListAsync(cancellationToken);

        if (notificacoes.Count == 0)
            return 0;

        var agora = DateTime.UtcNow;

        foreach (var notificacao in notificacoes)
        {
            notificacao.Ativo = true;
            notificacao.DataAtualizacao = agora;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return notificacoes.Count;
    }

    public async Task<int> ExcluirNotificacoesArquivadasEmLoteAsync(
        ArquivarNotificacoesEmLoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var notificacoes = await AplicarFiltrosAtividade(
                _context.Set<NotificacaoUsuario>(),
                ativo: false,
                request)
            .OrderByDescending(x => x.DataCriacao)
            .Take(request.Limite)
            .ToListAsync(cancellationToken);

        if (notificacoes.Count == 0)
            return 0;

        _context.Set<NotificacaoUsuario>().RemoveRange(notificacoes);
        await _context.SaveChangesAsync(cancellationToken);

        return notificacoes.Count;
    }

    public async Task<PreviewArquivamentoNotificacoesResponse> PreviewArquivamentoNotificacoesAsync(
        ArquivarNotificacoesEmLoteRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PreviewAtualizacaoAtividadeNotificacoesAsync(
            request,
            ativo: true,
            cancellationToken);
    }

    public async Task<PreviewArquivamentoNotificacoesResponse> PreviewRestauracaoNotificacoesAsync(
        ArquivarNotificacoesEmLoteRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PreviewAtualizacaoAtividadeNotificacoesAsync(
            request,
            ativo: false,
            cancellationToken);
    }

    public async Task<PreviewArquivamentoNotificacoesResponse> PreviewExclusaoNotificacoesArquivadasAsync(
        ArquivarNotificacoesEmLoteRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PreviewAtualizacaoAtividadeNotificacoesAsync(
            request,
            ativo: false,
            cancellationToken);
    }

    public async Task<PreviewExclusaoNotificacoesAntigasResponse> ObterAntigasExclusaoNotificacoesArquivadasAsync(
        ArquivarNotificacoesEmLoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = AplicarFiltrosAtividade(
            _context.Set<NotificacaoUsuario>().AsNoTracking(),
            ativo: false,
            request);

        var quantidadeCandidata = await query.CountAsync(cancellationToken);

        var antigas = await query
            .OrderBy(x => x.DataCriacao)
            .Take(Math.Min(request.Limite, 20))
            .Select(x => new NotificacaoAdminResponse
            {
                Id = x.Id,
                UsuarioId = x.UsuarioId,
                NomeUsuario = x.Usuario.Nome,
                EmailUsuario = x.Usuario.Email,
                Tipo = x.Tipo,
                Titulo = x.Titulo,
                Mensagem = x.Mensagem,
                ReferenciaId = x.ReferenciaId,
                Lida = x.DataLeitura != null,
                DataCriacao = x.DataCriacao,
                DataLeitura = x.DataLeitura
            })
            .ToListAsync(cancellationToken);

        return new PreviewExclusaoNotificacoesAntigasResponse
        {
            QuantidadeCandidata = quantidadeCandidata,
            Antigas = antigas
        };
    }

    public async Task<NotificacaoResumoOperacionalResponse> ObterResumoOperacionalNotificacoesAsync(
        Guid? usuarioId = null,
        TipoNotificacao? tipoNotificacao = null,
        DateTime? dataCriacaoInicial = null,
        DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        return await ObterResumoOperacionalNotificacoesPorAtividadeAsync(
            ativo: true,
            usuarioId,
            tipoNotificacao,
            dataCriacaoInicial,
            dataCriacaoFinal,
            cancellationToken);
    }

    public async Task<NotificacaoResumoOperacionalResponse> ObterResumoOperacionalNotificacoesArquivadasAsync(
        Guid? usuarioId = null,
        TipoNotificacao? tipoNotificacao = null,
        DateTime? dataCriacaoInicial = null,
        DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        return await ObterResumoOperacionalNotificacoesPorAtividadeAsync(
            ativo: false,
            usuarioId,
            tipoNotificacao,
            dataCriacaoInicial,
            dataCriacaoFinal,
            cancellationToken);
    }

    public async Task<NotificacaoResumoOperacionalResponse> ObterResumoOperacionalExclusaoNotificacoesArquivadasAsync(
        Guid? usuarioId = null,
        TipoNotificacao? tipoNotificacao = null,
        DateTime? dataCriacaoInicial = null,
        DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        return await ObterResumoOperacionalNotificacoesArquivadasAsync(
            usuarioId,
            tipoNotificacao,
            dataCriacaoInicial,
            dataCriacaoFinal,
            cancellationToken);
    }

    public async Task<NotificacaoArquivadaResumoIdadeResponse> ObterResumoIdadeExclusaoNotificacoesArquivadasAsync(
        Guid? usuarioId = null,
        TipoNotificacao? tipoNotificacao = null,
        DateTime? dataCriacaoInicial = null,
        DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<NotificacaoUsuario>()
            .AsNoTracking()
            .Where(x => !x.Ativo);

        if (usuarioId.HasValue)
            query = query.Where(x => x.UsuarioId == usuarioId.Value);

        if (tipoNotificacao.HasValue)
            query = query.Where(x => x.Tipo == tipoNotificacao.Value);

        if (dataCriacaoInicial.HasValue)
            query = query.Where(x => x.DataCriacao >= dataCriacaoInicial.Value);

        if (dataCriacaoFinal.HasValue)
            query = query.Where(x => x.DataCriacao <= dataCriacaoFinal.Value);

        var agora = DateTime.UtcNow.Date;
        var datasCriacao = await query
            .Select(x => x.DataCriacao)
            .ToListAsync(cancellationToken);

        var faixas = new[]
        {
            new { Nome = "0-7", DiasIniciais = 0, DiasFinais = (int?)7 },
            new { Nome = "8-30", DiasIniciais = 8, DiasFinais = (int?)30 },
            new { Nome = "31-90", DiasIniciais = 31, DiasFinais = (int?)90 },
            new { Nome = "91+", DiasIniciais = 91, DiasFinais = (int?)null }
        };

        var itens = faixas
            .Select(faixa => new NotificacaoArquivadaFaixaIdadeItemResponse
            {
                Faixa = faixa.Nome,
                DiasIniciais = faixa.DiasIniciais,
                DiasFinais = faixa.DiasFinais,
                Quantidade = datasCriacao.Count(dataCriacao =>
                {
                    var dias = Math.Max(0, (agora - dataCriacao.Date).Days);
                    return dias >= faixa.DiasIniciais &&
                           (!faixa.DiasFinais.HasValue || dias <= faixa.DiasFinais.Value);
                })
            })
            .ToList();

        return new NotificacaoArquivadaResumoIdadeResponse
        {
            UsuarioId = usuarioId,
            TipoNotificacao = tipoNotificacao,
            DataCriacaoInicial = dataCriacaoInicial,
            DataCriacaoFinal = dataCriacaoFinal,
            TotalRegistros = datasCriacao.Count,
            Faixas = itens
        };
    }

    public async Task<NotificacaoArquivadaResumoTiposResponse> ObterResumoTiposExclusaoNotificacoesArquivadasAsync(
        Guid? usuarioId = null,
        TipoNotificacao? tipoNotificacao = null,
        DateTime? dataCriacaoInicial = null,
        DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<NotificacaoUsuario>()
            .AsNoTracking()
            .Where(x => !x.Ativo);

        if (usuarioId.HasValue)
            query = query.Where(x => x.UsuarioId == usuarioId.Value);

        if (tipoNotificacao.HasValue)
            query = query.Where(x => x.Tipo == tipoNotificacao.Value);

        if (dataCriacaoInicial.HasValue)
            query = query.Where(x => x.DataCriacao >= dataCriacaoInicial.Value);

        if (dataCriacaoFinal.HasValue)
            query = query.Where(x => x.DataCriacao <= dataCriacaoFinal.Value);

        var totalRegistros = await query.CountAsync(cancellationToken);
        var tipos = await query
            .GroupBy(x => x.Tipo)
            .Select(x => new NotificacaoResumoOperacionalTipoItemResponse
            {
                TipoNotificacao = x.Key,
                Total = x.Count(),
                Lidas = x.Count(y => y.DataLeitura != null),
                NaoLidas = x.Count(y => y.DataLeitura == null)
            })
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.TipoNotificacao)
            .ToListAsync(cancellationToken);

        return new NotificacaoArquivadaResumoTiposResponse
        {
            UsuarioId = usuarioId,
            TipoNotificacao = tipoNotificacao,
            DataCriacaoInicial = dataCriacaoInicial,
            DataCriacaoFinal = dataCriacaoFinal,
            TotalRegistros = totalRegistros,
            Tipos = tipos
        };
    }

    public async Task<NotificacaoArquivadaResumoUsuariosResponse> ObterResumoUsuariosExclusaoNotificacoesArquivadasAsync(
        Guid? usuarioId = null,
        TipoNotificacao? tipoNotificacao = null,
        DateTime? dataCriacaoInicial = null,
        DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<NotificacaoUsuario>()
            .AsNoTracking()
            .Include(x => x.Usuario)
            .Where(x => !x.Ativo);

        if (usuarioId.HasValue)
            query = query.Where(x => x.UsuarioId == usuarioId.Value);

        if (tipoNotificacao.HasValue)
            query = query.Where(x => x.Tipo == tipoNotificacao.Value);

        if (dataCriacaoInicial.HasValue)
            query = query.Where(x => x.DataCriacao >= dataCriacaoInicial.Value);

        if (dataCriacaoFinal.HasValue)
            query = query.Where(x => x.DataCriacao <= dataCriacaoFinal.Value);

        var totalRegistros = await query.CountAsync(cancellationToken);
        var usuarios = await query
            .GroupBy(x => new
            {
                x.UsuarioId,
                x.Usuario.Nome,
                x.Usuario.Email
            })
            .Select(x => new NotificacaoResumoOperacionalUsuarioItemResponse
            {
                UsuarioId = x.Key.UsuarioId,
                Nome = x.Key.Nome,
                Email = x.Key.Email,
                Total = x.Count(),
                Lidas = x.Count(y => y.DataLeitura != null),
                NaoLidas = x.Count(y => y.DataLeitura == null)
            })
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.Nome)
            .ToListAsync(cancellationToken);

        return new NotificacaoArquivadaResumoUsuariosResponse
        {
            UsuarioId = usuarioId,
            TipoNotificacao = tipoNotificacao,
            DataCriacaoInicial = dataCriacaoInicial,
            DataCriacaoFinal = dataCriacaoFinal,
            TotalRegistros = totalRegistros,
            Usuarios = usuarios
        };
    }

    public async Task<NotificacaoArquivadaMetricasSerieResponse> ObterSerieExclusaoNotificacoesArquivadasAsync(
        Guid? usuarioId = null,
        TipoNotificacao? tipoNotificacao = null,
        DateTime? dataCriacaoInicial = null,
        DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<NotificacaoUsuario>()
            .AsNoTracking()
            .Where(x => !x.Ativo);

        if (usuarioId.HasValue)
            query = query.Where(x => x.UsuarioId == usuarioId.Value);

        if (tipoNotificacao.HasValue)
            query = query.Where(x => x.Tipo == tipoNotificacao.Value);

        if (dataCriacaoInicial.HasValue)
            query = query.Where(x => x.DataCriacao >= dataCriacaoInicial.Value);

        if (dataCriacaoFinal.HasValue)
            query = query.Where(x => x.DataCriacao <= dataCriacaoFinal.Value);

        var totalRegistros = await query.CountAsync(cancellationToken);
        var itens = await query
            .GroupBy(x => x.DataCriacao.Date)
            .Select(x => new NotificacaoArquivadaMetricaSerieItemResponse
            {
                Data = x.Key,
                Quantidade = x.Count()
            })
            .OrderBy(x => x.Data)
            .ToListAsync(cancellationToken);

        return new NotificacaoArquivadaMetricasSerieResponse
        {
            UsuarioId = usuarioId,
            TipoNotificacao = tipoNotificacao,
            DataCriacaoInicial = dataCriacaoInicial,
            DataCriacaoFinal = dataCriacaoFinal,
            TotalRegistros = totalRegistros,
            Itens = itens
        };
    }

    public async Task<NotificacaoArquivadaResumoLeituraResponse> ObterResumoLeituraExclusaoNotificacoesArquivadasAsync(
        Guid? usuarioId = null,
        TipoNotificacao? tipoNotificacao = null,
        DateTime? dataCriacaoInicial = null,
        DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<NotificacaoUsuario>()
            .AsNoTracking()
            .Where(x => !x.Ativo);

        if (usuarioId.HasValue)
            query = query.Where(x => x.UsuarioId == usuarioId.Value);

        if (tipoNotificacao.HasValue)
            query = query.Where(x => x.Tipo == tipoNotificacao.Value);

        if (dataCriacaoInicial.HasValue)
            query = query.Where(x => x.DataCriacao >= dataCriacaoInicial.Value);

        if (dataCriacaoFinal.HasValue)
            query = query.Where(x => x.DataCriacao <= dataCriacaoFinal.Value);

        var totalRegistros = await query.CountAsync(cancellationToken);
        var lidas = await query.CountAsync(x => x.DataLeitura != null, cancellationToken);
        var naoLidas = totalRegistros - lidas;

        decimal percentualLidas = 0;
        decimal percentualNaoLidas = 0;

        if (totalRegistros > 0)
        {
            percentualLidas = Math.Round((decimal)lidas * 100m / totalRegistros, 2);
            percentualNaoLidas = Math.Round((decimal)naoLidas * 100m / totalRegistros, 2);
        }

        return new NotificacaoArquivadaResumoLeituraResponse
        {
            UsuarioId = usuarioId,
            TipoNotificacao = tipoNotificacao,
            DataCriacaoInicial = dataCriacaoInicial,
            DataCriacaoFinal = dataCriacaoFinal,
            TotalRegistros = totalRegistros,
            Lidas = lidas,
            NaoLidas = naoLidas,
            PercentualLidas = percentualLidas,
            PercentualNaoLidas = percentualNaoLidas
        };
    }

    public async Task<NotificacaoArquivadaResumoLimitesResponse> ObterResumoLimitesExclusaoNotificacoesArquivadasAsync(
        Guid? usuarioId = null,
        TipoNotificacao? tipoNotificacao = null,
        DateTime? dataCriacaoInicial = null,
        DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        var totalRegistros = await _context.Set<NotificacaoUsuario>()
            .AsNoTracking()
            .Where(x => !x.Ativo)
            .Where(x => !usuarioId.HasValue || x.UsuarioId == usuarioId.Value)
            .Where(x => !tipoNotificacao.HasValue || x.Tipo == tipoNotificacao.Value)
            .Where(x => !dataCriacaoInicial.HasValue || x.DataCriacao >= dataCriacaoInicial.Value)
            .Where(x => !dataCriacaoFinal.HasValue || x.DataCriacao <= dataCriacaoFinal.Value)
            .CountAsync(cancellationToken);

        var limitesSugeridos = new[] { 20, 100, 500 };
        var limites = limitesSugeridos
            .Select(limite => new NotificacaoArquivadaResumoLimiteItemResponse
            {
                Limite = limite,
                QuantidadeAplicada = Math.Min(totalRegistros, limite),
                AtingeTodoBacklog = totalRegistros <= limite
            })
            .ToList();

        var limiteRecomendado = totalRegistros <= 20
            ? 20
            : totalRegistros <= 100
                ? 100
                : 500;
        var quantidadeLotesEstimados = totalRegistros == 0
            ? 0
            : (int)Math.Ceiling(totalRegistros / (double)limiteRecomendado);
        var nivelOperacional = quantidadeLotesEstimados switch
        {
            0 => "baixo",
            1 => "baixo",
            <= 3 => "medio",
            _ => "alto"
        };

        return new NotificacaoArquivadaResumoLimitesResponse
        {
            UsuarioId = usuarioId,
            TipoNotificacao = tipoNotificacao,
            DataCriacaoInicial = dataCriacaoInicial,
            DataCriacaoFinal = dataCriacaoFinal,
            TotalRegistros = totalRegistros,
            LimiteRecomendado = limiteRecomendado,
            ModoSeguro = totalRegistros <= limiteRecomendado,
            QuantidadeLotesEstimados = quantidadeLotesEstimados,
            CapacidadePorExecucao = $"Ate {limiteRecomendado} notificacoes por execucao",
            NivelOperacional = nivelOperacional,
            Limites = limites
        };
    }

    public async Task<NotificacaoArquivadaExclusaoDashboardResponse> ObterDashboardExclusaoNotificacoesArquivadasAsync(
        Guid? usuarioId = null,
        TipoNotificacao? tipoNotificacao = null,
        DateTime? dataCriacaoInicial = null,
        DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        var resumo = await ObterResumoOperacionalExclusaoNotificacoesArquivadasAsync(
            usuarioId,
            tipoNotificacao,
            dataCriacaoInicial,
            dataCriacaoFinal,
            cancellationToken);

        var leitura = await ObterResumoLeituraExclusaoNotificacoesArquivadasAsync(
            usuarioId,
            tipoNotificacao,
            dataCriacaoInicial,
            dataCriacaoFinal,
            cancellationToken);

        var serie = await ObterSerieExclusaoNotificacoesArquivadasAsync(
            usuarioId,
            tipoNotificacao,
            dataCriacaoInicial,
            dataCriacaoFinal,
            cancellationToken);

        var idade = await ObterResumoIdadeExclusaoNotificacoesArquivadasAsync(
            usuarioId,
            tipoNotificacao,
            dataCriacaoInicial,
            dataCriacaoFinal,
            cancellationToken);

        var tipos = await ObterResumoTiposExclusaoNotificacoesArquivadasAsync(
            usuarioId,
            tipoNotificacao,
            dataCriacaoInicial,
            dataCriacaoFinal,
            cancellationToken);

        var usuarios = await ObterResumoUsuariosExclusaoNotificacoesArquivadasAsync(
            usuarioId,
            tipoNotificacao,
            dataCriacaoInicial,
            dataCriacaoFinal,
            cancellationToken);

        var limites = await ObterResumoLimitesExclusaoNotificacoesArquivadasAsync(
            usuarioId,
            tipoNotificacao,
            dataCriacaoInicial,
            dataCriacaoFinal,
            cancellationToken);

        var antigas = await ObterAntigasExclusaoNotificacoesArquivadasAsync(
            new ArquivarNotificacoesEmLoteRequest
            {
                UsuarioId = usuarioId,
                TipoNotificacao = tipoNotificacao,
                DataCriacaoInicial = dataCriacaoInicial,
                DataCriacaoFinal = dataCriacaoFinal,
                Limite = 20
            },
            cancellationToken);

        return new NotificacaoArquivadaExclusaoDashboardResponse
        {
            UsuarioId = usuarioId,
            TipoNotificacao = tipoNotificacao,
            DataCriacaoInicial = dataCriacaoInicial,
            DataCriacaoFinal = dataCriacaoFinal,
            Resumo = resumo,
            Leitura = leitura,
            Serie = serie,
            Idade = idade,
            Tipos = tipos,
            Usuarios = usuarios,
            Limites = limites,
            Antigas = antigas
        };
    }

    private async Task<NotificacaoResumoOperacionalResponse> ObterResumoOperacionalNotificacoesPorAtividadeAsync(
        bool ativo,
        Guid? usuarioId,
        TipoNotificacao? tipoNotificacao,
        DateTime? dataCriacaoInicial,
        DateTime? dataCriacaoFinal,
        CancellationToken cancellationToken)
    {
        var query = _context.Set<NotificacaoUsuario>()
            .AsNoTracking()
            .Where(x => x.Ativo == ativo);

        if (usuarioId.HasValue)
            query = query.Where(x => x.UsuarioId == usuarioId.Value);

        if (tipoNotificacao.HasValue)
            query = query.Where(x => x.Tipo == tipoNotificacao.Value);

        if (dataCriacaoInicial.HasValue)
            query = query.Where(x => x.DataCriacao >= dataCriacaoInicial.Value);

        if (dataCriacaoFinal.HasValue)
            query = query.Where(x => x.DataCriacao <= dataCriacaoFinal.Value);

        var totalRegistros = await query.CountAsync(cancellationToken);
        var lidas = await query.CountAsync(x => x.DataLeitura != null, cancellationToken);
        var naoLidas = await query.CountAsync(x => x.DataLeitura == null, cancellationToken);

        var topTipos = await query
            .GroupBy(x => x.Tipo)
            .Select(x => new NotificacaoResumoOperacionalTipoItemResponse
            {
                TipoNotificacao = x.Key,
                Total = x.Count(),
                Lidas = x.Count(y => y.DataLeitura != null),
                NaoLidas = x.Count(y => y.DataLeitura == null)
            })
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.TipoNotificacao)
            .Take(5)
            .ToListAsync(cancellationToken);

        var topUsuariosComNaoLidas = await query
            .Where(x => x.DataLeitura == null)
            .GroupBy(x => new
            {
                x.UsuarioId,
                x.Usuario.Nome,
                x.Usuario.Email
            })
            .Select(x => new NotificacaoResumoOperacionalUsuarioItemResponse
            {
                UsuarioId = x.Key.UsuarioId,
                Nome = x.Key.Nome,
                Email = x.Key.Email,
                Total = x.Count(),
                Lidas = 0,
                NaoLidas = x.Count()
            })
            .OrderByDescending(x => x.NaoLidas)
            .ThenBy(x => x.Email)
            .Take(5)
            .ToListAsync(cancellationToken);

        return new NotificacaoResumoOperacionalResponse
        {
            UsuarioId = usuarioId,
            TipoNotificacao = tipoNotificacao,
            DataCriacaoInicial = dataCriacaoInicial,
            DataCriacaoFinal = dataCriacaoFinal,
            TotalRegistros = totalRegistros,
            Lidas = lidas,
            NaoLidas = naoLidas,
            TopTipos = topTipos,
            TopUsuariosComNaoLidas = topUsuariosComNaoLidas
        };
    }

    public async Task<NotificacaoUsuarioDashboardResponse> ObterDashboardNotificacoesPorUsuarioAsync(
        Guid usuarioId,
        TipoNotificacao? tipoNotificacao = null,
        DateTime? dataCriacaoInicial = null,
        DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        return await ObterDashboardNotificacoesPorUsuarioEAtividadeAsync(
            usuarioId,
            ativo: true,
            tipoNotificacao,
            dataCriacaoInicial,
            dataCriacaoFinal,
            cancellationToken);
    }

    public async Task<NotificacaoUsuarioDashboardResponse> ObterDashboardNotificacoesArquivadasPorUsuarioAsync(
        Guid usuarioId,
        TipoNotificacao? tipoNotificacao = null,
        DateTime? dataCriacaoInicial = null,
        DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        return await ObterDashboardNotificacoesPorUsuarioEAtividadeAsync(
            usuarioId,
            ativo: false,
            tipoNotificacao,
            dataCriacaoInicial,
            dataCriacaoFinal,
            cancellationToken);
    }

    public async Task<NotificacaoUsuarioDashboardResponse> ObterDashboardExclusaoNotificacoesArquivadasPorUsuarioAsync(
        Guid usuarioId,
        TipoNotificacao? tipoNotificacao = null,
        DateTime? dataCriacaoInicial = null,
        DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        return await ObterDashboardNotificacoesArquivadasPorUsuarioAsync(
            usuarioId,
            tipoNotificacao,
            dataCriacaoInicial,
            dataCriacaoFinal,
            cancellationToken);
    }

    private async Task<NotificacaoUsuarioDashboardResponse> ObterDashboardNotificacoesPorUsuarioEAtividadeAsync(
        Guid usuarioId,
        bool ativo,
        TipoNotificacao? tipoNotificacao,
        DateTime? dataCriacaoInicial,
        DateTime? dataCriacaoFinal,
        CancellationToken cancellationToken)
    {
        var resumo = ativo
            ? await ObterResumoOperacionalNotificacoesAsync(
                usuarioId,
                tipoNotificacao,
                dataCriacaoInicial,
                dataCriacaoFinal,
                cancellationToken)
            : await ObterResumoOperacionalNotificacoesArquivadasAsync(
                usuarioId,
                tipoNotificacao,
                dataCriacaoInicial,
                dataCriacaoFinal,
                cancellationToken);

        var query = _context.Set<NotificacaoUsuario>()
            .AsNoTracking()
            .Where(x => x.Ativo == ativo && x.UsuarioId == usuarioId);

        if (tipoNotificacao.HasValue)
            query = query.Where(x => x.Tipo == tipoNotificacao.Value);

        if (dataCriacaoInicial.HasValue)
            query = query.Where(x => x.DataCriacao >= dataCriacaoInicial.Value);

        if (dataCriacaoFinal.HasValue)
            query = query.Where(x => x.DataCriacao <= dataCriacaoFinal.Value);

        var recentes = await query
            .OrderByDescending(x => x.DataCriacao)
            .Take(20)
            .Select(x => new NotificacaoAdminResponse
            {
                Id = x.Id,
                UsuarioId = x.UsuarioId,
                NomeUsuario = x.Usuario.Nome,
                EmailUsuario = x.Usuario.Email,
                Tipo = x.Tipo,
                Titulo = x.Titulo,
                Mensagem = x.Mensagem,
                ReferenciaId = x.ReferenciaId,
                Lida = x.DataLeitura != null,
                DataCriacao = x.DataCriacao,
                DataLeitura = x.DataLeitura
            })
            .ToListAsync(cancellationToken);

        return new NotificacaoUsuarioDashboardResponse
        {
            UsuarioId = usuarioId,
            TipoNotificacao = tipoNotificacao,
            DataCriacaoInicial = dataCriacaoInicial,
            DataCriacaoFinal = dataCriacaoFinal,
            Resumo = resumo,
            Recentes = recentes
        };
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

    public async Task<PaginacaoResponse<EmailNotificacaoOutboxResponse>> ListarEmailsOutboxAsync(
        BuscarEmailsOutboxRequest request,
        CancellationToken cancellationToken = default)
    {
        var pagina = request.Pagina <= 0 ? 1 : request.Pagina;
        var tamanhoPagina = request.TamanhoPagina <= 0 ? 20 : Math.Min(request.TamanhoPagina, 100);

        var query = AplicarFiltrosEmailsOutbox(request).AsNoTracking();

        query = AplicarOrdenacaoOutbox(query, request.OrdenarPor, request.OrdemDesc);

        var totalRegistros = await query.CountAsync(cancellationToken);

        var itens = await query
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
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

        return new PaginacaoResponse<EmailNotificacaoOutboxResponse>
        {
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalRegistros = totalRegistros,
            TotalPaginas = totalRegistros == 0 ? 0 : (int)Math.Ceiling(totalRegistros / (double)tamanhoPagina),
            Itens = itens
        };
    }

    public async Task<string> ExportarEmailsOutboxCsvAsync(
        ExportarEmailsOutboxRequest request,
        CancellationToken cancellationToken = default)
    {
        var emails = await AplicarFiltrosEmailsOutbox(request)
            .AsNoTracking()
            .OrderByDescending(x => x.DataCriacao)
            .Take(Math.Min(request.Limite, 5000))
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

        var csv = new StringBuilder();
        csv.AppendLine("Id,UsuarioId,TipoNotificacao,EmailDestino,Assunto,Status,TentativasProcessamento,DataCriacao,DataProcessamento,ProximaTentativaEm,ReferenciaId,UltimaMensagemErro");

        foreach (var email in emails)
        {
            csv.AppendLine(string.Join(",",
                EscaparCsv(email.Id),
                EscaparCsv(email.UsuarioId),
                EscaparCsv(email.TipoNotificacao),
                EscaparCsv(email.EmailDestino),
                EscaparCsv(email.Assunto),
                EscaparCsv(email.Status),
                EscaparCsv(email.TentativasProcessamento),
                EscaparCsv(email.DataCriacao),
                EscaparCsv(email.DataProcessamento),
                EscaparCsv(email.ProximaTentativaEm),
                EscaparCsv(email.ReferenciaId),
                EscaparCsv(email.UltimaMensagemErro)));
        }

        return csv.ToString();
    }

    private static IQueryable<EmailNotificacaoOutbox> AplicarOrdenacaoOutbox(
        IQueryable<EmailNotificacaoOutbox> query,
        string? ordenarPor,
        bool ordemDesc)
    {
        var campo = ordenarPor?.Trim().ToLowerInvariant();

        return campo switch
        {
            "status" => ordemDesc
                ? query.OrderByDescending(x => x.Status).ThenByDescending(x => x.DataCriacao)
                : query.OrderBy(x => x.Status).ThenBy(x => x.DataCriacao),
            "emaildestino" => ordemDesc
                ? query.OrderByDescending(x => x.EmailDestino).ThenByDescending(x => x.DataCriacao)
                : query.OrderBy(x => x.EmailDestino).ThenBy(x => x.DataCriacao),
            "tiponotificacao" => ordemDesc
                ? query.OrderByDescending(x => x.TipoNotificacao).ThenByDescending(x => x.DataCriacao)
                : query.OrderBy(x => x.TipoNotificacao).ThenBy(x => x.DataCriacao),
            "tentativasprocessamento" => ordemDesc
                ? query.OrderByDescending(x => x.TentativasProcessamento).ThenByDescending(x => x.DataCriacao)
                : query.OrderBy(x => x.TentativasProcessamento).ThenBy(x => x.DataCriacao),
            "dataprocessamento" => ordemDesc
                ? query.OrderByDescending(x => x.DataProcessamento).ThenByDescending(x => x.DataCriacao)
                : query.OrderBy(x => x.DataProcessamento).ThenBy(x => x.DataCriacao),
            _ => ordemDesc
                ? query.OrderByDescending(x => x.DataCriacao)
                : query.OrderBy(x => x.DataCriacao)
        };
    }

    public async Task<EmailNotificacaoOutboxResponse?> ObterEmailOutboxPorIdAsync(
        Guid emailId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<EmailNotificacaoOutbox>()
            .AsNoTracking()
            .Where(x => x.Id == emailId && x.Ativo)
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
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<EmailNotificacaoOutboxResponse?> CancelarEmailOutboxAsync(
        Guid emailId,
        CancellationToken cancellationToken = default)
    {
        var email = await _context.Set<EmailNotificacaoOutbox>()
            .FirstOrDefaultAsync(x => x.Id == emailId && x.Ativo, cancellationToken);

        if (email is null)
            return null;

        if (email.Status == StatusEmailNotificacao.Enviado)
            throw new InvalidOperationException("E-mail já enviado não pode ser cancelado.");

        if (email.Status != StatusEmailNotificacao.Cancelado)
        {
            var agora = DateTime.UtcNow;
            email.Status = StatusEmailNotificacao.Cancelado;
            email.ProximaTentativaEm = null;
            email.DataAtualizacao = agora;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return await ObterEmailOutboxPorIdAsync(emailId, cancellationToken);
    }

    public async Task<int> CancelarEmailsOutboxEmLoteAsync(
        AtualizarEmailsOutboxEmLoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var agora = DateTime.UtcNow;
        var emails = await AplicarFiltrosEmailsOutbox(request)
            .Where(x => x.Status != StatusEmailNotificacao.Enviado && x.Status != StatusEmailNotificacao.Cancelado)
            .OrderBy(x => x.DataCriacao)
            .Take(Math.Min(request.Limite, 500))
            .ToListAsync(cancellationToken);

        foreach (var email in emails)
        {
            email.Status = StatusEmailNotificacao.Cancelado;
            email.ProximaTentativaEm = null;
            email.DataAtualizacao = agora;
        }

        if (emails.Count > 0)
            await _context.SaveChangesAsync(cancellationToken);

        return emails.Count;
    }

    public async Task<int> ReabrirEmailsOutboxEmLoteAsync(
        AtualizarEmailsOutboxEmLoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var agora = DateTime.UtcNow;
        var emails = await AplicarFiltrosEmailsOutbox(request)
            .Where(x => x.Status != StatusEmailNotificacao.Enviado && x.Status != StatusEmailNotificacao.Pendente)
            .OrderBy(x => x.DataCriacao)
            .Take(Math.Min(request.Limite, 500))
            .ToListAsync(cancellationToken);

        foreach (var email in emails)
        {
            email.Status = StatusEmailNotificacao.Pendente;
            email.ProximaTentativaEm = agora;
            email.DataAtualizacao = agora;
        }

        if (emails.Count > 0)
            await _context.SaveChangesAsync(cancellationToken);

        return emails.Count;
    }

    public async Task<int> ReprocessarEmailsOutboxEmLoteAsync(
        AtualizarEmailsOutboxEmLoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var emails = await AplicarFiltrosEmailsOutbox(request)
            .Where(x =>
                x.Status == StatusEmailNotificacao.Pendente ||
                x.Status == StatusEmailNotificacao.Falha)
            .Where(x => x.ProximaTentativaEm == null || x.ProximaTentativaEm <= DateTime.UtcNow)
            .OrderBy(x => x.DataCriacao)
            .Take(Math.Min(request.Limite, 500))
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

    public async Task<EmailNotificacaoOutboxResponse?> ReabrirEmailOutboxAsync(
        Guid emailId,
        CancellationToken cancellationToken = default)
    {
        var email = await _context.Set<EmailNotificacaoOutbox>()
            .FirstOrDefaultAsync(x => x.Id == emailId && x.Ativo, cancellationToken);

        if (email is null)
            return null;

        if (email.Status == StatusEmailNotificacao.Enviado)
            throw new InvalidOperationException("E-mail já enviado não pode ser reaberto.");

        if (email.Status != StatusEmailNotificacao.Pendente)
        {
            var agora = DateTime.UtcNow;
            email.Status = StatusEmailNotificacao.Pendente;
            email.ProximaTentativaEm = agora;
            email.DataAtualizacao = agora;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return await ObterEmailOutboxPorIdAsync(emailId, cancellationToken);
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
        BuscarMetricasEmailsOutboxRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = AplicarFiltrosMetricasEmailsOutbox(request);

        var totalRegistros = await query.CountAsync(cancellationToken);

        var itens = await query
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
            TotalRegistros = totalRegistros,
            TipoNotificacao = request.TipoNotificacao,
            EmailDestino = request.EmailDestino,
            DataCriacaoInicial = request.DataCriacaoInicial,
            DataCriacaoFinal = request.DataCriacaoFinal,
            Itens = itens
        };
    }

    public async Task<EmailNotificacaoResumoOperacionalResponse> ObterResumoOperacionalEmailsOutboxAsync(
        BuscarMetricasEmailsOutboxRequest request,
        CancellationToken cancellationToken = default)
    {
        var agora = DateTime.UtcNow;
        var query = AplicarFiltrosMetricasEmailsOutbox(request);

        var totalRegistros = await query.CountAsync(cancellationToken);
        var pendentes = await query.CountAsync(x => x.Status == StatusEmailNotificacao.Pendente, cancellationToken);
        var enviados = await query.CountAsync(x => x.Status == StatusEmailNotificacao.Enviado, cancellationToken);
        var falhas = await query.CountAsync(x => x.Status == StatusEmailNotificacao.Falha, cancellationToken);
        var cancelados = await query.CountAsync(x => x.Status == StatusEmailNotificacao.Cancelado, cancellationToken);

        var topTiposComFalha = await query
            .Where(x => x.Status == StatusEmailNotificacao.Falha)
            .GroupBy(x => x.TipoNotificacao)
            .Select(x => new EmailNotificacaoResumoOperacionalTipoFalhaResponse
            {
                TipoNotificacao = x.Key,
                QuantidadeFalhas = x.Count()
            })
            .OrderByDescending(x => x.QuantidadeFalhas)
            .ThenBy(x => x.TipoNotificacao)
            .Take(5)
            .ToListAsync(cancellationToken);

        var topDestinatariosComFalha = await query
            .Where(x => x.Status == StatusEmailNotificacao.Falha)
            .GroupBy(x => new { x.UsuarioId, x.EmailDestino })
            .Select(x => new EmailNotificacaoResumoOperacionalDestinatarioFalhaResponse
            {
                UsuarioId = x.Key.UsuarioId,
                EmailDestino = x.Key.EmailDestino,
                QuantidadeFalhas = x.Count()
            })
            .OrderByDescending(x => x.QuantidadeFalhas)
            .ThenBy(x => x.EmailDestino)
            .Take(5)
            .ToListAsync(cancellationToken);

        var prontosParaProcessar = await query.CountAsync(
            x => (x.Status == StatusEmailNotificacao.Pendente || x.Status == StatusEmailNotificacao.Falha) &&
                 (!x.ProximaTentativaEm.HasValue || x.ProximaTentativaEm <= agora),
            cancellationToken);

        var aguardandoProximaTentativa = await query.CountAsync(
            x => (x.Status == StatusEmailNotificacao.Pendente || x.Status == StatusEmailNotificacao.Falha) &&
                 x.ProximaTentativaEm.HasValue &&
                 x.ProximaTentativaEm > agora,
            cancellationToken);

        return new EmailNotificacaoResumoOperacionalResponse
        {
            UsuarioId = request.UsuarioId,
            TipoNotificacao = request.TipoNotificacao,
            EmailDestino = request.EmailDestino,
            DataCriacaoInicial = request.DataCriacaoInicial,
            DataCriacaoFinal = request.DataCriacaoFinal,
            TotalRegistros = totalRegistros,
            Pendentes = pendentes,
            Enviados = enviados,
            Falhas = falhas,
            Cancelados = cancelados,
            ProntosParaProcessar = prontosParaProcessar,
            AguardandoProximaTentativa = aguardandoProximaTentativa,
            TopTiposComFalha = topTiposComFalha,
            TopDestinatariosComFalha = topDestinatariosComFalha
        };
    }

    public async Task<EmailNotificacaoMetricasSerieResponse> ObterMetricasSerieEmailsOutboxAsync(
        BuscarMetricasEmailsOutboxRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = AplicarFiltrosMetricasEmailsOutbox(request);
        var totalRegistros = await query.CountAsync(cancellationToken);

        var itens = await query
            .GroupBy(x => new
            {
                Data = x.DataCriacao.Date,
                x.TipoNotificacao,
                x.Status
            })
            .Select(x => new EmailNotificacaoMetricaSerieItemResponse
            {
                Data = x.Key.Data,
                TipoNotificacao = x.Key.TipoNotificacao,
                Status = x.Key.Status,
                Quantidade = x.Count()
            })
            .OrderBy(x => x.Data)
            .ThenBy(x => x.TipoNotificacao)
            .ThenBy(x => x.Status)
            .ToListAsync(cancellationToken);

        return new EmailNotificacaoMetricasSerieResponse
        {
            TotalRegistros = totalRegistros,
            TipoNotificacao = request.TipoNotificacao,
            EmailDestino = request.EmailDestino,
            DataCriacaoInicial = request.DataCriacaoInicial,
            DataCriacaoFinal = request.DataCriacaoFinal,
            Itens = itens
        };
    }

    public async Task<EmailNotificacaoDestinatariosMetricasResponse> ObterMetricasDestinatariosEmailsOutboxAsync(
        BuscarMetricasEmailsOutboxRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = AplicarFiltrosMetricasEmailsOutbox(request);
        var totalRegistros = await query.CountAsync(cancellationToken);

        var itens = await query
            .GroupBy(x => new
            {
                x.UsuarioId,
                x.EmailDestino
            })
            .Select(x => new EmailNotificacaoDestinatarioMetricaItemResponse
            {
                UsuarioId = x.Key.UsuarioId,
                EmailDestino = x.Key.EmailDestino,
                Total = x.Count(),
                Pendentes = x.Count(y => y.Status == StatusEmailNotificacao.Pendente),
                Enviados = x.Count(y => y.Status == StatusEmailNotificacao.Enviado),
                Falhas = x.Count(y => y.Status == StatusEmailNotificacao.Falha),
                Cancelados = x.Count(y => y.Status == StatusEmailNotificacao.Cancelado)
            })
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.EmailDestino)
            .ToListAsync(cancellationToken);

        return new EmailNotificacaoDestinatariosMetricasResponse
        {
            TotalRegistros = totalRegistros,
            TotalDestinatarios = itens.Count,
            TipoNotificacao = request.TipoNotificacao,
            EmailDestino = request.EmailDestino,
            DataCriacaoInicial = request.DataCriacaoInicial,
            DataCriacaoFinal = request.DataCriacaoFinal,
            Itens = itens
        };
    }

    public async Task<EmailNotificacaoTiposMetricasResponse> ObterMetricasTiposEmailsOutboxAsync(
        BuscarMetricasEmailsOutboxRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = AplicarFiltrosMetricasEmailsOutbox(request);
        var totalRegistros = await query.CountAsync(cancellationToken);

        var itens = await query
            .GroupBy(x => x.TipoNotificacao)
            .Select(x => new EmailNotificacaoTipoMetricaItemResponse
            {
                TipoNotificacao = x.Key,
                Total = x.Count(),
                Pendentes = x.Count(y => y.Status == StatusEmailNotificacao.Pendente),
                Enviados = x.Count(y => y.Status == StatusEmailNotificacao.Enviado),
                Falhas = x.Count(y => y.Status == StatusEmailNotificacao.Falha),
                Cancelados = x.Count(y => y.Status == StatusEmailNotificacao.Cancelado)
            })
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.TipoNotificacao)
            .ToListAsync(cancellationToken);

        return new EmailNotificacaoTiposMetricasResponse
        {
            TotalRegistros = totalRegistros,
            EmailDestino = request.EmailDestino,
            DataCriacaoInicial = request.DataCriacaoInicial,
            DataCriacaoFinal = request.DataCriacaoFinal,
            Itens = itens
        };
    }

    public async Task<EmailNotificacaoDashboardResponse> ObterDashboardEmailsOutboxAsync(
        BuscarMetricasEmailsOutboxRequest request,
        CancellationToken cancellationToken = default)
    {
        var resumo = await ObterMetricasEmailsOutboxAsync(request, cancellationToken);
        var serie = await ObterMetricasSerieEmailsOutboxAsync(request, cancellationToken);
        var destinatarios = await ObterMetricasDestinatariosEmailsOutboxAsync(request, cancellationToken);
        var tipos = await ObterMetricasTiposEmailsOutboxAsync(request, cancellationToken);

        return new EmailNotificacaoDashboardResponse
        {
            TipoNotificacao = request.TipoNotificacao,
            EmailDestino = request.EmailDestino,
            DataCriacaoInicial = request.DataCriacaoInicial,
            DataCriacaoFinal = request.DataCriacaoFinal,
            Resumo = resumo,
            Serie = serie,
            Destinatarios = destinatarios,
            Tipos = tipos
        };
    }

    public async Task<EmailNotificacaoUsuarioDashboardResponse> ObterDashboardEmailsOutboxPorUsuarioAsync(
        Guid usuarioId,
        BuscarMetricasEmailsOutboxRequest request,
        CancellationToken cancellationToken = default)
    {
        request.UsuarioId = usuarioId;

        var resumo = await ObterMetricasEmailsOutboxAsync(request, cancellationToken);
        var serie = await ObterMetricasSerieEmailsOutboxAsync(request, cancellationToken);
        var tipos = await ObterMetricasTiposEmailsOutboxAsync(request, cancellationToken);

        var recentes = await _context.Set<EmailNotificacaoOutbox>()
            .AsNoTracking()
            .Where(x => x.Ativo && x.UsuarioId == usuarioId)
            .Where(x => !request.TipoNotificacao.HasValue || x.TipoNotificacao == request.TipoNotificacao.Value)
            .Where(x => !request.DataCriacaoInicial.HasValue || x.DataCriacao >= request.DataCriacaoInicial.Value)
            .Where(x => !request.DataCriacaoFinal.HasValue || x.DataCriacao <= request.DataCriacaoFinal.Value)
            .OrderByDescending(x => x.DataCriacao)
            .Take(20)
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

        return new EmailNotificacaoUsuarioDashboardResponse
        {
            UsuarioId = usuarioId,
            TipoNotificacao = request.TipoNotificacao,
            DataCriacaoInicial = request.DataCriacaoInicial,
            DataCriacaoFinal = request.DataCriacaoFinal,
            Resumo = resumo,
            Serie = serie,
            Tipos = tipos,
            Recentes = recentes
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

    private IQueryable<EmailNotificacaoOutbox> AplicarFiltrosMetricasEmailsOutbox(
        BuscarMetricasEmailsOutboxRequest request)
    {
        var query = _context.Set<EmailNotificacaoOutbox>()
            .AsNoTracking()
            .Where(x => x.Ativo);

        if (request.UsuarioId.HasValue)
            query = query.Where(x => x.UsuarioId == request.UsuarioId.Value);

        if (request.TipoNotificacao.HasValue)
            query = query.Where(x => x.TipoNotificacao == request.TipoNotificacao.Value);

        if (!string.IsNullOrWhiteSpace(request.EmailDestino))
        {
            var emailNormalizado = request.EmailDestino.Trim().ToLowerInvariant();
            query = query.Where(x => x.EmailDestino.ToLower().Contains(emailNormalizado));
        }

        if (request.DataCriacaoInicial.HasValue)
            query = query.Where(x => x.DataCriacao >= request.DataCriacaoInicial.Value);

        if (request.DataCriacaoFinal.HasValue)
            query = query.Where(x => x.DataCriacao <= request.DataCriacaoFinal.Value);

        return query;
    }

    private IQueryable<EmailNotificacaoOutbox> AplicarFiltrosEmailsOutbox(BuscarEmailsOutboxRequest request)
    {
        var query = _context.Set<EmailNotificacaoOutbox>()
            .Where(x => x.Ativo);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (request.UsuarioId.HasValue)
            query = query.Where(x => x.UsuarioId == request.UsuarioId.Value);

        if (request.TipoNotificacao.HasValue)
            query = query.Where(x => x.TipoNotificacao == request.TipoNotificacao.Value);

        if (!string.IsNullOrWhiteSpace(request.EmailDestino))
        {
            var emailNormalizado = request.EmailDestino.Trim().ToLowerInvariant();
            query = query.Where(x => x.EmailDestino.ToLower().Contains(emailNormalizado));
        }

        if (request.DataCriacaoInicial.HasValue)
            query = query.Where(x => x.DataCriacao >= request.DataCriacaoInicial.Value);

        if (request.DataCriacaoFinal.HasValue)
            query = query.Where(x => x.DataCriacao <= request.DataCriacaoFinal.Value);

        return query;
    }

    private IQueryable<EmailNotificacaoOutbox> AplicarFiltrosEmailsOutbox(ExportarEmailsOutboxRequest request)
    {
        var query = _context.Set<EmailNotificacaoOutbox>()
            .Where(x => x.Ativo);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (request.UsuarioId.HasValue)
            query = query.Where(x => x.UsuarioId == request.UsuarioId.Value);

        if (request.TipoNotificacao.HasValue)
            query = query.Where(x => x.TipoNotificacao == request.TipoNotificacao.Value);

        if (!string.IsNullOrWhiteSpace(request.EmailDestino))
        {
            var emailNormalizado = request.EmailDestino.Trim().ToLowerInvariant();
            query = query.Where(x => x.EmailDestino.ToLower().Contains(emailNormalizado));
        }

        if (request.DataCriacaoInicial.HasValue)
            query = query.Where(x => x.DataCriacao >= request.DataCriacaoInicial.Value);

        if (request.DataCriacaoFinal.HasValue)
            query = query.Where(x => x.DataCriacao <= request.DataCriacaoFinal.Value);

        return query;
    }

    private static string EscaparCsv(object? valor)
    {
        if (valor is null)
            return "\"\"";

        var texto = valor switch
        {
            DateTime dateTime => dateTime.ToString("O"),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("O"),
            _ => valor.ToString() ?? string.Empty
        };

        return $"\"{texto.Replace("\"", "\"\"")}\"";
    }

    private IQueryable<EmailNotificacaoOutbox> AplicarFiltrosEmailsOutbox(AtualizarEmailsOutboxEmLoteRequest request)
    {
        var query = _context.Set<EmailNotificacaoOutbox>()
            .Where(x => x.Ativo);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (request.UsuarioId.HasValue)
            query = query.Where(x => x.UsuarioId == request.UsuarioId.Value);

        if (request.TipoNotificacao.HasValue)
            query = query.Where(x => x.TipoNotificacao == request.TipoNotificacao.Value);

        if (!string.IsNullOrWhiteSpace(request.EmailDestino))
        {
            var emailNormalizado = request.EmailDestino.Trim().ToLowerInvariant();
            query = query.Where(x => x.EmailDestino.ToLower().Contains(emailNormalizado));
        }

        if (request.DataCriacaoInicial.HasValue)
            query = query.Where(x => x.DataCriacao >= request.DataCriacaoInicial.Value);

        if (request.DataCriacaoFinal.HasValue)
            query = query.Where(x => x.DataCriacao <= request.DataCriacaoFinal.Value);

        return query;
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

    private static IQueryable<NotificacaoUsuario> AplicarFiltrosAtividade(
        IQueryable<NotificacaoUsuario> query,
        bool ativo,
        ArquivarNotificacoesEmLoteRequest request)
    {
        query = query.Where(x => x.Ativo == ativo);

        if (request.UsuarioId.HasValue)
            query = query.Where(x => x.UsuarioId == request.UsuarioId.Value);

        if (request.TipoNotificacao.HasValue)
            query = query.Where(x => x.Tipo == request.TipoNotificacao.Value);

        if (request.Lida.HasValue)
            query = request.Lida.Value ? query.Where(x => x.DataLeitura != null) : query.Where(x => x.DataLeitura == null);

        if (request.DataCriacaoInicial.HasValue)
            query = query.Where(x => x.DataCriacao >= request.DataCriacaoInicial.Value);

        if (request.DataCriacaoFinal.HasValue)
            query = query.Where(x => x.DataCriacao <= request.DataCriacaoFinal.Value);

        return query;
    }

    private async Task<PreviewArquivamentoNotificacoesResponse> PreviewAtualizacaoAtividadeNotificacoesAsync(
        ArquivarNotificacoesEmLoteRequest request,
        bool ativo,
        CancellationToken cancellationToken)
    {
        var query = AplicarFiltrosAtividade(
            _context.Set<NotificacaoUsuario>().AsNoTracking(),
            ativo,
            request);

        var quantidadeCandidata = await query.CountAsync(cancellationToken);

        var recentes = await query
            .OrderByDescending(x => x.DataCriacao)
            .Take(Math.Min(request.Limite, 20))
            .Select(x => new NotificacaoAdminResponse
            {
                Id = x.Id,
                UsuarioId = x.UsuarioId,
                NomeUsuario = x.Usuario.Nome,
                EmailUsuario = x.Usuario.Email,
                Tipo = x.Tipo,
                Titulo = x.Titulo,
                Mensagem = x.Mensagem,
                ReferenciaId = x.ReferenciaId,
                Lida = x.DataLeitura != null,
                DataCriacao = x.DataCriacao,
                DataLeitura = x.DataLeitura
            })
            .ToListAsync(cancellationToken);

        return new PreviewArquivamentoNotificacoesResponse
        {
            QuantidadeCandidata = quantidadeCandidata,
            Recentes = recentes
        };
    }

    private static TipoNotificacao[] ListarTiposSuportados()
    {
        return Enum.GetValues<TipoNotificacao>()
            .OrderBy(x => (int)x)
            .ToArray();
    }
}
