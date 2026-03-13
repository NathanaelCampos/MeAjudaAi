using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.Interfaces.Admin;
using MeAjudaAi.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Api.Controllers;

[ApiController]
[Route("api/admin/impulsionamentos")]
[Authorize(Roles = "Administrador")]
public class AdminImpulsionamentosController : ControllerBase
{
    private readonly IAdminImpulsionamentoService _adminImpulsionamentoService;

    public AdminImpulsionamentosController(IAdminImpulsionamentoService adminImpulsionamentoService)
    {
        _adminImpulsionamentoService = adminImpulsionamentoService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginacaoResponse<ImpulsionamentoAdminListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Buscar(
        [FromQuery] string? termo = null,
        [FromQuery] Guid? profissionalId = null,
        [FromQuery] Guid? planoImpulsionamentoId = null,
        [FromQuery] StatusImpulsionamento? status = null,
        [FromQuery] DateTime? dataInicioInicial = null,
        [FromQuery] DateTime? dataInicioFinal = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _adminImpulsionamentoService.BuscarAsync(new BuscarImpulsionamentosAdminRequest
        {
            Termo = termo,
            ProfissionalId = profissionalId,
            PlanoImpulsionamentoId = planoImpulsionamentoId,
            Status = status,
            DataInicioInicial = dataInicioInicial,
            DataInicioFinal = dataInicioFinal,
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina
        }, cancellationToken);

        return Ok(response);
    }

    [HttpGet("{impulsionamentoId:guid}")]
    [ProducesResponseType(typeof(ImpulsionamentoAdminDetalheResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterPorId(
        Guid impulsionamentoId,
        CancellationToken cancellationToken = default)
    {
        var response = await _adminImpulsionamentoService.ObterPorIdAsync(impulsionamentoId, cancellationToken);

        if (response is null)
            return NotFound(new MensagemErroResponse { Mensagem = "Impulsionamento não encontrado." });

        return Ok(response);
    }

    [HttpGet("{impulsionamentoId:guid}/dashboard")]
    [ProducesResponseType(typeof(ImpulsionamentoAdminDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterDashboard(
        Guid impulsionamentoId,
        CancellationToken cancellationToken = default)
    {
        var response = await _adminImpulsionamentoService.ObterDashboardAsync(impulsionamentoId, cancellationToken);

        if (response is null)
            return NotFound(new MensagemErroResponse { Mensagem = "Impulsionamento não encontrado." });

        return Ok(response);
    }
}
