using MeAjudaAi.Api.Extensions;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.DTOs.Impulsionamentos;
using MeAjudaAi.Application.Interfaces.Impulsionamentos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Api.Controllers;

[ApiController]
[Route("api/impulsionamentos")]
public class ImpulsionamentosController : ControllerBase
{
    private readonly IImpulsionamentoService _impulsionamentoService;
    private readonly IWebhookPagamentoMetricsService _webhookPagamentoMetricsService;

    public ImpulsionamentosController(
        IImpulsionamentoService impulsionamentoService,
        IWebhookPagamentoMetricsService webhookPagamentoMetricsService)
    {
        _impulsionamentoService = impulsionamentoService;
        _webhookPagamentoMetricsService = webhookPagamentoMetricsService;
    }

    [HttpGet("planos")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<PlanoImpulsionamentoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPlanos(CancellationToken cancellationToken = default)
    {
        var response = await _impulsionamentoService.ListarPlanosAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPost("contratar")]
    [Authorize]
    [ProducesResponseType(typeof(ImpulsionamentoProfissionalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroValidacaoResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Contratar(
        [FromBody] ContratarPlanoImpulsionamentoRequest request,
        CancellationToken cancellationToken = default)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        var response = await _impulsionamentoService.ContratarPlanoAsync(
            usuarioId.Value,
            request,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("meus")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<ImpulsionamentoProfissionalResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListarMeus(
        CancellationToken cancellationToken = default)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        var response = await _impulsionamentoService.ListarMeusImpulsionamentosAsync(
            usuarioId.Value,
            cancellationToken);

        return Ok(response);
    }

    [HttpPut("{impulsionamentoId:guid}/confirmar-pagamento")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(ImpulsionamentoProfissionalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ConfirmarPagamento(
        Guid impulsionamentoId,
        CancellationToken cancellationToken = default)
    {
        var response = await _impulsionamentoService.ConfirmarPagamentoAsync(
            impulsionamentoId,
            cancellationToken);

        return Ok(response);
    }

    [HttpPost("confirmar-pagamento")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(ImpulsionamentoProfissionalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroValidacaoResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ConfirmarPagamentoPorCodigoReferencia(
        [FromBody] ConfirmarPagamentoImpulsionamentoRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _impulsionamentoService.ConfirmarPagamentoPorCodigoReferenciaAsync(
            request.CodigoReferenciaPagamento,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("webhooks")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(MeAjudaAi.Application.DTOs.Common.PaginacaoResponse<WebhookPagamentoImpulsionamentoEventoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListarWebhooks(
        [FromQuery] BuscarWebhookPagamentosRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _impulsionamentoService.ListarWebhooksAsync(
            request,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("webhooks/metricas")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(WebhookPagamentoMetricasResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult ObterMetricasWebhooks()
    {
        var response = _webhookPagamentoMetricsService.ObterSnapshot();
        return Ok(response);
    }
}
