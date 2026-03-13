using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.DTOs.Impulsionamentos;
using MeAjudaAi.Application.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Api.Controllers;

[ApiController]
[Route("api/admin/webhooks/pagamentos")]
[Authorize(Roles = "Administrador")]
public class AdminWebhooksPagamentosController : ControllerBase
{
    private readonly IAdminWebhookPagamentoService _adminWebhookPagamentoService;

    public AdminWebhooksPagamentosController(IAdminWebhookPagamentoService adminWebhookPagamentoService)
    {
        _adminWebhookPagamentoService = adminWebhookPagamentoService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginacaoResponse<WebhookPagamentoImpulsionamentoEventoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Buscar(
        [FromQuery] string? eventoExternoId = null,
        [FromQuery] string? codigoReferenciaPagamento = null,
        [FromQuery] string? provedor = null,
        [FromQuery] bool? processadoComSucesso = null,
        [FromQuery] Guid? impulsionamentoProfissionalId = null,
        [FromQuery] DateTime? dataCriacaoInicial = null,
        [FromQuery] DateTime? dataCriacaoFinal = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _adminWebhookPagamentoService.BuscarAsync(new BuscarWebhooksPagamentoAdminRequest
        {
            EventoExternoId = eventoExternoId,
            CodigoReferenciaPagamento = codigoReferenciaPagamento,
            Provedor = provedor,
            ProcessadoComSucesso = processadoComSucesso,
            ImpulsionamentoProfissionalId = impulsionamentoProfissionalId,
            DataCriacaoInicial = dataCriacaoInicial,
            DataCriacaoFinal = dataCriacaoFinal,
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina
        }, cancellationToken);

        return Ok(response);
    }

    [HttpGet("{webhookId:guid}")]
    [ProducesResponseType(typeof(WebhookPagamentoAdminDetalheResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterPorId(
        Guid webhookId,
        CancellationToken cancellationToken = default)
    {
        var response = await _adminWebhookPagamentoService.ObterPorIdAsync(webhookId, cancellationToken);

        if (response is null)
            return NotFound(new MensagemErroResponse { Mensagem = "Webhook de pagamento não encontrado." });

        return Ok(response);
    }
}
