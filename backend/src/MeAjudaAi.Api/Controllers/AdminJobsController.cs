using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Api.Controllers;

[ApiController]
[Route("api/admin/jobs")]
[Authorize(Roles = "Administrador")]
public class AdminJobsController : ControllerBase
{
    private readonly IAdminJobService _adminJobService;

    public AdminJobsController(IAdminJobService adminJobService)
    {
        _adminJobService = adminJobService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BackgroundJobAdminItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken = default)
    {
        var response = await _adminJobService.ListarAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPost("{jobId}/executar")]
    [ProducesResponseType(typeof(ExecutarBackgroundJobAdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Executar(string jobId, CancellationToken cancellationToken = default)
    {
        var response = await _adminJobService.ExecutarAsync(jobId, cancellationToken);
        if (response is null)
            return NotFound(new MensagemErroResponse { Mensagem = "Job não encontrado." });

        return Ok(response);
    }
}
