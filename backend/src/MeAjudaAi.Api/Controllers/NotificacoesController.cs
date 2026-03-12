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

    public NotificacoesController(INotificacaoService notificacaoService)
    {
        _notificacaoService = notificacaoService;
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
    [ProducesResponseType(typeof(IReadOnlyList<EmailNotificacaoOutboxResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListarEmailsOutbox(
        [FromQuery] StatusEmailNotificacao? status = null,
        [FromQuery] Guid? usuarioId = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _notificacaoService.ListarEmailsOutboxAsync(
            status,
            usuarioId,
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
