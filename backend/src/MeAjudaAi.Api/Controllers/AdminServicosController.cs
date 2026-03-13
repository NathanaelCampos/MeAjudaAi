using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.Interfaces.Admin;
using MeAjudaAi.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Api.Controllers;

[ApiController]
[Route("api/admin/servicos")]
[Authorize(Roles = "Administrador")]
public class AdminServicosController : ControllerBase
{
    private readonly IAdminServicoService _adminServicoService;

    public AdminServicosController(IAdminServicoService adminServicoService)
    {
        _adminServicoService = adminServicoService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginacaoResponse<ServicoAdminListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Buscar(
        [FromQuery] string? termo = null,
        [FromQuery] Guid? clienteId = null,
        [FromQuery] Guid? profissionalId = null,
        [FromQuery] StatusServico? status = null,
        [FromQuery] DateTime? dataCriacaoInicial = null,
        [FromQuery] DateTime? dataCriacaoFinal = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _adminServicoService.BuscarAsync(new BuscarServicosAdminRequest
        {
            Termo = termo,
            ClienteId = clienteId,
            ProfissionalId = profissionalId,
            Status = status,
            DataCriacaoInicial = dataCriacaoInicial,
            DataCriacaoFinal = dataCriacaoFinal,
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina
        }, cancellationToken);

        return Ok(response);
    }

    [HttpGet("{servicoId:guid}")]
    [ProducesResponseType(typeof(ServicoAdminDetalheResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterPorId(
        Guid servicoId,
        CancellationToken cancellationToken = default)
    {
        var response = await _adminServicoService.ObterPorIdAsync(servicoId, cancellationToken);

        if (response is null)
            return NotFound(new MensagemErroResponse { Mensagem = "Serviço não encontrado." });

        return Ok(response);
    }

    [HttpGet("{servicoId:guid}/dashboard")]
    [ProducesResponseType(typeof(ServicoAdminDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterDashboard(
        Guid servicoId,
        CancellationToken cancellationToken = default)
    {
        var response = await _adminServicoService.ObterDashboardAsync(servicoId, cancellationToken);

        if (response is null)
            return NotFound(new MensagemErroResponse { Mensagem = "Serviço não encontrado." });

        return Ok(response);
    }
}
