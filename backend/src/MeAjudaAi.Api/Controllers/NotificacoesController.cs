using MeAjudaAi.Api.Extensions;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.DTOs.Notificacoes;
using MeAjudaAi.Application.Interfaces.Notificacoes;
using MeAjudaAi.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Api.Controllers;

[ApiController]
[Route("api/notificacoes")]
[Authorize]
public class NotificacoesController : ControllerBase
{
    private readonly INotificacaoService _notificacaoService;
    private readonly INotificacaoRetentionService _notificacaoRetentionService;
    private readonly INotificacaoRetentionMetricsService _notificacaoRetentionMetricsService;
    private readonly IEmailNotificacaoTemplateRenderer _emailTemplateRenderer;

    public NotificacoesController(
        INotificacaoService notificacaoService,
        INotificacaoRetentionService notificacaoRetentionService,
        INotificacaoRetentionMetricsService notificacaoRetentionMetricsService,
        IEmailNotificacaoTemplateRenderer emailTemplateRenderer)
    {
        _notificacaoService = notificacaoService;
        _notificacaoRetentionService = notificacaoRetentionService;
        _notificacaoRetentionMetricsService = notificacaoRetentionMetricsService;
        _emailTemplateRenderer = emailTemplateRenderer;
    }

    [HttpGet("minhas")]
    [ProducesResponseType(typeof(IReadOnlyList<NotificacaoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListarMinhas(
        [FromQuery] bool somenteNaoLidas = false,
        CancellationToken cancellationToken = default)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        var response = await _notificacaoService.ListarMinhasAsync(
            usuarioId.Value,
            somenteNaoLidas,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(PaginacaoResponse<NotificacaoAdminResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Listar(
        [FromQuery] Guid? usuarioId = null,
        [FromQuery] TipoNotificacao? tipoNotificacao = null,
        [FromQuery] bool? lida = null,
        [FromQuery] DateTime? dataCriacaoInicial = null,
        [FromQuery] DateTime? dataCriacaoFinal = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ListarNotificacoesAsync(
            new BuscarNotificacoesRequest
            {
                UsuarioId = usuarioId,
                TipoNotificacao = tipoNotificacao,
                Lida = lida,
                DataCriacaoInicial = dataCriacaoInicial,
                DataCriacaoFinal = dataCriacaoFinal,
                Pagina = pagina,
                TamanhoPagina = tamanhoPagina
            },
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("arquivadas")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(PaginacaoResponse<NotificacaoAdminResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListarArquivadas(
        [FromQuery] Guid? usuarioId = null,
        [FromQuery] TipoNotificacao? tipoNotificacao = null,
        [FromQuery] bool? lida = null,
        [FromQuery] DateTime? dataCriacaoInicial = null,
        [FromQuery] DateTime? dataCriacaoFinal = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ListarNotificacoesArquivadasAsync(
            new BuscarNotificacoesRequest
            {
                UsuarioId = usuarioId,
                TipoNotificacao = tipoNotificacao,
                Lida = lida,
                DataCriacaoInicial = dataCriacaoInicial,
                DataCriacaoFinal = dataCriacaoFinal,
                Pagina = pagina,
                TamanhoPagina = tamanhoPagina
            },
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("exportar")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroValidacaoResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Exportar(
        [FromQuery] ExportarNotificacoesRequest request,
        CancellationToken cancellationToken = default)
    {
        var csv = await _notificacaoService.ExportarNotificacoesCsvAsync(request, cancellationToken);

        return File(
            System.Text.Encoding.UTF8.GetBytes(csv),
            "text/csv; charset=utf-8",
            $"notificacoes-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    [HttpGet("arquivadas/exportar")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroValidacaoResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExportarArquivadas(
        [FromQuery] ExportarNotificacoesRequest request,
        CancellationToken cancellationToken = default)
    {
        var csv = await _notificacaoService.ExportarNotificacoesArquivadasCsvAsync(request, cancellationToken);

        return File(
            System.Text.Encoding.UTF8.GetBytes(csv),
            "text/csv; charset=utf-8",
            $"notificacoes-arquivadas-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    [HttpGet("{notificacaoId:guid}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(NotificacaoAdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterPorId(
        Guid notificacaoId,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ObterNotificacaoPorIdAsync(notificacaoId, cancellationToken);

        if (response is null)
        {
            return NotFound(new MensagemErroResponse
            {
                Mensagem = "Notificação não encontrada."
            });
        }

        return Ok(response);
    }

    [HttpGet("arquivadas/{notificacaoId:guid}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(NotificacaoAdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterArquivadaPorId(
        Guid notificacaoId,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ObterNotificacaoArquivadaPorIdAsync(notificacaoId, cancellationToken);

        if (response is null)
        {
            return NotFound(new MensagemErroResponse
            {
                Mensagem = "Notificação arquivada não encontrada."
            });
        }

        return Ok(response);
    }

    [HttpPut("marcar-lidas-lote")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(AtualizarEmailsOutboxEmLoteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroValidacaoResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarcarComoLidasEmLote(
        [FromBody] MarcarNotificacoesComoLidasEmLoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var quantidade = await _notificacaoService.MarcarNotificacoesComoLidasEmLoteAsync(request, cancellationToken);

        return Ok(new AtualizarEmailsOutboxEmLoteResponse
        {
            QuantidadeAfetada = quantidade
        });
    }

    [HttpPut("arquivar-lote")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(AtualizarEmailsOutboxEmLoteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroValidacaoResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ArquivarEmLote(
        [FromBody] ArquivarNotificacoesEmLoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var quantidade = await _notificacaoService.ArquivarNotificacoesEmLoteAsync(request, cancellationToken);

        return Ok(new AtualizarEmailsOutboxEmLoteResponse
        {
            QuantidadeAfetada = quantidade
        });
    }

    [HttpPut("restaurar-lote")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(AtualizarEmailsOutboxEmLoteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroValidacaoResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RestaurarEmLote(
        [FromBody] ArquivarNotificacoesEmLoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var quantidade = await _notificacaoService.RestaurarNotificacoesEmLoteAsync(request, cancellationToken);

        return Ok(new AtualizarEmailsOutboxEmLoteResponse
        {
            QuantidadeAfetada = quantidade
        });
    }

    [HttpPost("arquivadas/excluir-lote")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(AtualizarEmailsOutboxEmLoteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroValidacaoResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExcluirArquivadasEmLote(
        [FromBody] ArquivarNotificacoesEmLoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var quantidade = await _notificacaoService.ExcluirNotificacoesArquivadasEmLoteAsync(request, cancellationToken);

        return Ok(new AtualizarEmailsOutboxEmLoteResponse
        {
            QuantidadeAfetada = quantidade
        });
    }

    [HttpPost("arquivadas/excluir-lote/preview")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(PreviewArquivamentoNotificacoesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroValidacaoResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PreviewExcluirArquivadasEmLote(
        [FromBody] ArquivarNotificacoesEmLoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.PreviewExclusaoNotificacoesArquivadasAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("arquivar-lote/preview")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(PreviewArquivamentoNotificacoesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroValidacaoResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PreviewArquivarEmLote(
        [FromBody] ArquivarNotificacoesEmLoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.PreviewArquivamentoNotificacoesAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("restaurar-lote/preview")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(PreviewArquivamentoNotificacoesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroValidacaoResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PreviewRestaurarEmLote(
        [FromBody] ArquivarNotificacoesEmLoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.PreviewRestauracaoNotificacoesAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("retencao/executar")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(ExecutarRetencaoNotificacoesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExecutarRetencao(
        CancellationToken cancellationToken = default)
    {
        var quantidade = await _notificacaoRetentionService.ProcessarRetencaoAsync(cancellationToken);

        return Ok(new ExecutarRetencaoNotificacoesResponse
        {
            QuantidadeArquivada = quantidade
        });
    }

    [HttpGet("retencao/resumo")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(RetencaoNotificacoesResumoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult ObterResumoRetencao(
        [FromServices] Microsoft.Extensions.Options.IOptions<MeAjudaAi.Infrastructure.Configurations.NotificacaoInternaRetentionOptions> options)
    {
        var response = _notificacaoRetentionMetricsService.ObterResumo();
        response.Habilitada = options.Value.Habilitada;
        response.DiasRetencao = options.Value.DiasRetencao;
        response.LoteProcessamento = options.Value.LoteProcessamento;
        response.SomenteLidas = options.Value.SomenteLidas;
        return Ok(response);
    }

    [HttpGet("minhas/nao-lidas/quantidade")]
    [ProducesResponseType(typeof(QuantidadeNotificacoesNaoLidasResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterQuantidadeNaoLidas(
        CancellationToken cancellationToken = default)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        var response = await _notificacaoService.ObterQuantidadeNaoLidasAsync(
            usuarioId.Value,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("resumo-operacional")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(NotificacaoResumoOperacionalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterResumoOperacional(
        [FromQuery] Guid? usuarioId = null,
        [FromQuery] TipoNotificacao? tipoNotificacao = null,
        [FromQuery] DateTime? dataCriacaoInicial = null,
        [FromQuery] DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ObterResumoOperacionalNotificacoesAsync(
            usuarioId,
            tipoNotificacao,
            dataCriacaoInicial,
            dataCriacaoFinal,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("arquivadas/resumo-operacional")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(NotificacaoResumoOperacionalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterResumoOperacionalArquivadas(
        [FromQuery] Guid? usuarioId = null,
        [FromQuery] TipoNotificacao? tipoNotificacao = null,
        [FromQuery] DateTime? dataCriacaoInicial = null,
        [FromQuery] DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ObterResumoOperacionalNotificacoesArquivadasAsync(
            usuarioId,
            tipoNotificacao,
            dataCriacaoInicial,
            dataCriacaoFinal,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("arquivadas/excluir-lote/resumo-operacional")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(NotificacaoResumoOperacionalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterResumoOperacionalExclusaoArquivadas(
        [FromQuery] Guid? usuarioId = null,
        [FromQuery] TipoNotificacao? tipoNotificacao = null,
        [FromQuery] DateTime? dataCriacaoInicial = null,
        [FromQuery] DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ObterResumoOperacionalExclusaoNotificacoesArquivadasAsync(
            usuarioId,
            tipoNotificacao,
            dataCriacaoInicial,
            dataCriacaoFinal,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("arquivadas/excluir-lote/resumo-idade")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(NotificacaoArquivadaResumoIdadeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterResumoIdadeExclusaoArquivadas(
        [FromQuery] Guid? usuarioId = null,
        [FromQuery] TipoNotificacao? tipoNotificacao = null,
        [FromQuery] DateTime? dataCriacaoInicial = null,
        [FromQuery] DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ObterResumoIdadeExclusaoNotificacoesArquivadasAsync(
            usuarioId,
            tipoNotificacao,
            dataCriacaoInicial,
            dataCriacaoFinal,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("arquivadas/excluir-lote/resumo-tipos")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(NotificacaoArquivadaResumoTiposResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterResumoTiposExclusaoArquivadas(
        [FromQuery] Guid? usuarioId = null,
        [FromQuery] TipoNotificacao? tipoNotificacao = null,
        [FromQuery] DateTime? dataCriacaoInicial = null,
        [FromQuery] DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ObterResumoTiposExclusaoNotificacoesArquivadasAsync(
            usuarioId,
            tipoNotificacao,
            dataCriacaoInicial,
            dataCriacaoFinal,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("arquivadas/excluir-lote/resumo-usuarios")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(NotificacaoArquivadaResumoUsuariosResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterResumoUsuariosExclusaoArquivadas(
        [FromQuery] Guid? usuarioId = null,
        [FromQuery] TipoNotificacao? tipoNotificacao = null,
        [FromQuery] DateTime? dataCriacaoInicial = null,
        [FromQuery] DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ObterResumoUsuariosExclusaoNotificacoesArquivadasAsync(
            usuarioId,
            tipoNotificacao,
            dataCriacaoInicial,
            dataCriacaoFinal,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("arquivadas/excluir-lote/serie")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(NotificacaoArquivadaMetricasSerieResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterSerieExclusaoArquivadas(
        [FromQuery] Guid? usuarioId = null,
        [FromQuery] TipoNotificacao? tipoNotificacao = null,
        [FromQuery] DateTime? dataCriacaoInicial = null,
        [FromQuery] DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ObterSerieExclusaoNotificacoesArquivadasAsync(
            usuarioId,
            tipoNotificacao,
            dataCriacaoInicial,
            dataCriacaoFinal,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("arquivadas/excluir-lote/resumo-leitura")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(NotificacaoArquivadaResumoLeituraResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterResumoLeituraExclusaoArquivadas(
        [FromQuery] Guid? usuarioId = null,
        [FromQuery] TipoNotificacao? tipoNotificacao = null,
        [FromQuery] DateTime? dataCriacaoInicial = null,
        [FromQuery] DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ObterResumoLeituraExclusaoNotificacoesArquivadasAsync(
            usuarioId,
            tipoNotificacao,
            dataCriacaoInicial,
            dataCriacaoFinal,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("arquivadas/excluir-lote/resumo-limites")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(NotificacaoArquivadaResumoLimitesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterResumoLimitesExclusaoArquivadas(
        [FromQuery] Guid? usuarioId = null,
        [FromQuery] TipoNotificacao? tipoNotificacao = null,
        [FromQuery] DateTime? dataCriacaoInicial = null,
        [FromQuery] DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ObterResumoLimitesExclusaoNotificacoesArquivadasAsync(
            usuarioId,
            tipoNotificacao,
            dataCriacaoInicial,
            dataCriacaoFinal,
            cancellationToken);

        return Ok(response);
    }

    [HttpPost("arquivadas/excluir-lote/antigas")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(PreviewExclusaoNotificacoesAntigasResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroValidacaoResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterAntigasExclusaoArquivadas(
        [FromBody] ArquivarNotificacoesEmLoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ObterAntigasExclusaoNotificacoesArquivadasAsync(
            request,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("arquivadas/excluir-lote/dashboard")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(NotificacaoArquivadaExclusaoDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterDashboardExclusaoArquivadas(
        [FromQuery] Guid? usuarioId = null,
        [FromQuery] TipoNotificacao? tipoNotificacao = null,
        [FromQuery] DateTime? dataCriacaoInicial = null,
        [FromQuery] DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ObterDashboardExclusaoNotificacoesArquivadasAsync(
            usuarioId,
            tipoNotificacao,
            dataCriacaoInicial,
            dataCriacaoFinal,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("usuarios/{usuarioId:guid}/dashboard")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(NotificacaoUsuarioDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterDashboardPorUsuario(
        Guid usuarioId,
        [FromQuery] TipoNotificacao? tipoNotificacao = null,
        [FromQuery] DateTime? dataCriacaoInicial = null,
        [FromQuery] DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ObterDashboardNotificacoesPorUsuarioAsync(
            usuarioId,
            tipoNotificacao,
            dataCriacaoInicial,
            dataCriacaoFinal,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("arquivadas/usuarios/{usuarioId:guid}/dashboard")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(NotificacaoUsuarioDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterDashboardArquivadasPorUsuario(
        Guid usuarioId,
        [FromQuery] TipoNotificacao? tipoNotificacao = null,
        [FromQuery] DateTime? dataCriacaoInicial = null,
        [FromQuery] DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ObterDashboardNotificacoesArquivadasPorUsuarioAsync(
            usuarioId,
            tipoNotificacao,
            dataCriacaoInicial,
            dataCriacaoFinal,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("arquivadas/usuarios/{usuarioId:guid}/excluir-lote/dashboard")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(NotificacaoUsuarioDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterDashboardExclusaoArquivadasPorUsuario(
        Guid usuarioId,
        [FromQuery] TipoNotificacao? tipoNotificacao = null,
        [FromQuery] DateTime? dataCriacaoInicial = null,
        [FromQuery] DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ObterDashboardExclusaoNotificacoesArquivadasPorUsuarioAsync(
            usuarioId,
            tipoNotificacao,
            dataCriacaoInicial,
            dataCriacaoFinal,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("minhas/preferencias")]
    [ProducesResponseType(typeof(IReadOnlyList<PreferenciaNotificacaoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListarPreferencias(
        CancellationToken cancellationToken = default)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        var response = await _notificacaoService.ListarPreferenciasAsync(
            usuarioId.Value,
            cancellationToken);

        return Ok(response);
    }

    [HttpPut("minhas/preferencias")]
    [ProducesResponseType(typeof(IReadOnlyList<PreferenciaNotificacaoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroValidacaoResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AtualizarPreferencias(
        [FromBody] AtualizarPreferenciasNotificacaoRequest request,
        CancellationToken cancellationToken = default)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        var response = await _notificacaoService.AtualizarPreferenciasAsync(
            usuarioId.Value,
            request.Preferencias,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("emails")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(MeAjudaAi.Application.DTOs.Common.PaginacaoResponse<EmailNotificacaoOutboxResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListarEmailsOutbox(
        [FromQuery] StatusEmailNotificacao? status = null,
        [FromQuery] Guid? usuarioId = null,
        [FromQuery] TipoNotificacao? tipoNotificacao = null,
        [FromQuery] string? emailDestino = null,
        [FromQuery] DateTime? dataCriacaoInicial = null,
        [FromQuery] DateTime? dataCriacaoFinal = null,
        [FromQuery] string? ordenarPor = "dataCriacao",
        [FromQuery] bool ordemDesc = true,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ListarEmailsOutboxAsync(
            new BuscarEmailsOutboxRequest
            {
                Status = status,
                UsuarioId = usuarioId,
                TipoNotificacao = tipoNotificacao,
                EmailDestino = emailDestino,
                DataCriacaoInicial = dataCriacaoInicial,
                DataCriacaoFinal = dataCriacaoFinal,
                OrdenarPor = ordenarPor,
                OrdemDesc = ordemDesc,
                Pagina = pagina,
                TamanhoPagina = tamanhoPagina
            },
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("emails/exportar")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroValidacaoResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExportarEmailsOutbox(
        [FromQuery] ExportarEmailsOutboxRequest request,
        CancellationToken cancellationToken = default)
    {
        var csv = await _notificacaoService.ExportarEmailsOutboxCsvAsync(request, cancellationToken);

        return File(
            System.Text.Encoding.UTF8.GetBytes(csv),
            "text/csv; charset=utf-8",
            $"emails-outbox-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    [HttpGet("emails/{emailId:guid}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(EmailNotificacaoOutboxResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterEmailOutboxPorId(
        Guid emailId,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ObterEmailOutboxPorIdAsync(emailId, cancellationToken);

        if (response is null)
        {
            return NotFound(new MensagemErroResponse
            {
                Mensagem = "E-mail do outbox não encontrado."
            });
        }

        return Ok(response);
    }

    [HttpPut("emails/{emailId:guid}/cancelar")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(EmailNotificacaoOutboxResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelarEmailOutbox(
        Guid emailId,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.CancelarEmailOutboxAsync(emailId, cancellationToken);

        if (response is null)
        {
            return NotFound(new MensagemErroResponse
            {
                Mensagem = "E-mail do outbox não encontrado."
            });
        }

        return Ok(response);
    }

    [HttpPut("emails/{emailId:guid}/reabrir")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(EmailNotificacaoOutboxResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ReabrirEmailOutbox(
        Guid emailId,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ReabrirEmailOutboxAsync(emailId, cancellationToken);

        if (response is null)
        {
            return NotFound(new MensagemErroResponse
            {
                Mensagem = "E-mail do outbox não encontrado."
            });
        }

        return Ok(response);
    }

    [HttpPut("emails/cancelar-lote")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(AtualizarEmailsOutboxEmLoteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroValidacaoResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelarEmailsOutboxEmLote(
        [FromBody] AtualizarEmailsOutboxEmLoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var quantidade = await _notificacaoService.CancelarEmailsOutboxEmLoteAsync(request, cancellationToken);

        return Ok(new AtualizarEmailsOutboxEmLoteResponse
        {
            QuantidadeAfetada = quantidade
        });
    }

    [HttpPut("emails/reabrir-lote")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(AtualizarEmailsOutboxEmLoteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroValidacaoResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ReabrirEmailsOutboxEmLote(
        [FromBody] AtualizarEmailsOutboxEmLoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var quantidade = await _notificacaoService.ReabrirEmailsOutboxEmLoteAsync(request, cancellationToken);

        return Ok(new AtualizarEmailsOutboxEmLoteResponse
        {
            QuantidadeAfetada = quantidade
        });
    }

    [HttpPost("emails/reprocessar-lote")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(AtualizarEmailsOutboxEmLoteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroValidacaoResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ReprocessarEmailsOutboxEmLote(
        [FromBody] AtualizarEmailsOutboxEmLoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var quantidade = await _notificacaoService.ReprocessarEmailsOutboxEmLoteAsync(request, cancellationToken);

        return Ok(new AtualizarEmailsOutboxEmLoteResponse
        {
            QuantidadeAfetada = quantidade
        });
    }

    [HttpGet("emails/metricas")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(EmailNotificacaoMetricasResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterMetricasEmailsOutbox(
        [FromQuery] TipoNotificacao? tipoNotificacao = null,
        [FromQuery] string? emailDestino = null,
        [FromQuery] DateTime? dataCriacaoInicial = null,
        [FromQuery] DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ObterMetricasEmailsOutboxAsync(
            new BuscarMetricasEmailsOutboxRequest
            {
                TipoNotificacao = tipoNotificacao,
                EmailDestino = emailDestino,
                DataCriacaoInicial = dataCriacaoInicial,
                DataCriacaoFinal = dataCriacaoFinal
            },
            cancellationToken);
        return Ok(response);
    }

    [HttpGet("emails/resumo-operacional")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(EmailNotificacaoResumoOperacionalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterResumoOperacionalEmailsOutbox(
        [FromQuery] Guid? usuarioId = null,
        [FromQuery] TipoNotificacao? tipoNotificacao = null,
        [FromQuery] string? emailDestino = null,
        [FromQuery] DateTime? dataCriacaoInicial = null,
        [FromQuery] DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ObterResumoOperacionalEmailsOutboxAsync(
            new BuscarMetricasEmailsOutboxRequest
            {
                UsuarioId = usuarioId,
                TipoNotificacao = tipoNotificacao,
                EmailDestino = emailDestino,
                DataCriacaoInicial = dataCriacaoInicial,
                DataCriacaoFinal = dataCriacaoFinal
            },
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("emails/metricas/serie")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(EmailNotificacaoMetricasSerieResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterMetricasSerieEmailsOutbox(
        [FromQuery] TipoNotificacao? tipoNotificacao = null,
        [FromQuery] string? emailDestino = null,
        [FromQuery] DateTime? dataCriacaoInicial = null,
        [FromQuery] DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ObterMetricasSerieEmailsOutboxAsync(
            new BuscarMetricasEmailsOutboxRequest
            {
                TipoNotificacao = tipoNotificacao,
                EmailDestino = emailDestino,
                DataCriacaoInicial = dataCriacaoInicial,
                DataCriacaoFinal = dataCriacaoFinal
            },
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("emails/metricas/destinatarios")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(EmailNotificacaoDestinatariosMetricasResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterMetricasDestinatariosEmailsOutbox(
        [FromQuery] TipoNotificacao? tipoNotificacao = null,
        [FromQuery] string? emailDestino = null,
        [FromQuery] DateTime? dataCriacaoInicial = null,
        [FromQuery] DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ObterMetricasDestinatariosEmailsOutboxAsync(
            new BuscarMetricasEmailsOutboxRequest
            {
                TipoNotificacao = tipoNotificacao,
                EmailDestino = emailDestino,
                DataCriacaoInicial = dataCriacaoInicial,
                DataCriacaoFinal = dataCriacaoFinal
            },
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("emails/metricas/tipos")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(EmailNotificacaoTiposMetricasResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterMetricasTiposEmailsOutbox(
        [FromQuery] string? emailDestino = null,
        [FromQuery] DateTime? dataCriacaoInicial = null,
        [FromQuery] DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ObterMetricasTiposEmailsOutboxAsync(
            new BuscarMetricasEmailsOutboxRequest
            {
                EmailDestino = emailDestino,
                DataCriacaoInicial = dataCriacaoInicial,
                DataCriacaoFinal = dataCriacaoFinal
            },
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("emails/dashboard")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(EmailNotificacaoDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterDashboardEmailsOutbox(
        [FromQuery] TipoNotificacao? tipoNotificacao = null,
        [FromQuery] string? emailDestino = null,
        [FromQuery] DateTime? dataCriacaoInicial = null,
        [FromQuery] DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ObterDashboardEmailsOutboxAsync(
            new BuscarMetricasEmailsOutboxRequest
            {
                TipoNotificacao = tipoNotificacao,
                EmailDestino = emailDestino,
                DataCriacaoInicial = dataCriacaoInicial,
                DataCriacaoFinal = dataCriacaoFinal
            },
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("emails/usuarios/{usuarioId:guid}/dashboard")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(EmailNotificacaoUsuarioDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterDashboardEmailsOutboxPorUsuario(
        Guid usuarioId,
        [FromQuery] TipoNotificacao? tipoNotificacao = null,
        [FromQuery] DateTime? dataCriacaoInicial = null,
        [FromQuery] DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ObterDashboardEmailsOutboxPorUsuarioAsync(
            usuarioId,
            new BuscarMetricasEmailsOutboxRequest
            {
                TipoNotificacao = tipoNotificacao,
                DataCriacaoInicial = dataCriacaoInicial,
                DataCriacaoFinal = dataCriacaoFinal
            },
            cancellationToken);

        return Ok(response);
    }

    [HttpPost("emails/reprocessar")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(ReprocessarEmailsOutboxResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ReprocessarEmailsOutbox(
        CancellationToken cancellationToken = default)
    {
        var quantidade = await _notificacaoService.ReprocessarEmailsOutboxAsync(cancellationToken);

        return Ok(new ReprocessarEmailsOutboxResponse
        {
            QuantidadeProcessada = quantidade
        });
    }

    [HttpPost("emails/preview")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(PreviewEmailNotificacaoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroValidacaoResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult PreviewEmail([FromBody] PreviewEmailNotificacaoRequest request)
    {
        var email = new Domain.Entities.EmailNotificacaoOutbox
        {
            TipoNotificacao = request.TipoNotificacao,
            Assunto = request.Assunto.Trim(),
            Corpo = request.Corpo.Trim(),
            ReferenciaId = request.ReferenciaId,
            EmailDestino = "preview@local.test"
        };

        return Ok(new PreviewEmailNotificacaoResponse
        {
            TipoNotificacao = request.TipoNotificacao,
            Assunto = email.Assunto,
            ReferenciaId = request.ReferenciaId,
            Html = _emailTemplateRenderer.RenderizarHtml(email)
        });
    }

    [HttpPut("{notificacaoId:guid}/marcar-lida")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarcarComoLida(
        Guid notificacaoId,
        CancellationToken cancellationToken = default)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        var response = await _notificacaoService.MarcarComoLidaAsync(
            usuarioId.Value,
            notificacaoId,
            cancellationToken);

        if (response is null)
        {
            return NotFound(new MensagemErroResponse
            {
                Mensagem = "Notificação não encontrada."
            });
        }

        Response.StatusCode = StatusCodes.Status204NoContent;
        return NoContent();
    }

    [HttpPut("minhas/marcar-todas-lidas")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarcarTodasComoLidas(
        CancellationToken cancellationToken = default)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        await _notificacaoService.MarcarTodasComoLidasAsync(
            usuarioId.Value,
            cancellationToken);

        return NoContent();
    }
}
