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
