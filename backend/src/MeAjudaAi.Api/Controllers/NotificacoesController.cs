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
    private readonly IEmailNotificacaoTemplateRenderer _emailTemplateRenderer;

    public NotificacoesController(
        INotificacaoService notificacaoService,
        IEmailNotificacaoTemplateRenderer emailTemplateRenderer)
    {
        _notificacaoService = notificacaoService;
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
                Pagina = pagina,
                TamanhoPagina = tamanhoPagina
            },
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("emails/metricas")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(EmailNotificacaoMetricasResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterMetricasEmailsOutbox(
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ObterMetricasEmailsOutboxAsync(cancellationToken);
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
    [ProducesResponseType(typeof(NotificacaoResponse), StatusCodes.Status200OK)]
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

        return Ok(response);
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
