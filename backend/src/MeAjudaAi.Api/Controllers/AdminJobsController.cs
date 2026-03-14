using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Api.Extensions;
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

    [HttpGet("fila")]
    [ProducesResponseType(typeof(IReadOnlyList<BackgroundJobFilaItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListarFila([FromQuery] string? jobId = null, [FromQuery] string? status = null, [FromQuery] int? limit = null, CancellationToken cancellationToken = default)
    {
        var response = await _adminJobService.ListarFilaAsync(jobId, status, limit, cancellationToken);
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

    [HttpPost("{jobId}/enfileirar")]
    [ProducesResponseType(typeof(EnfileirarBackgroundJobAdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Enfileirar(string jobId, CancellationToken cancellationToken = default)
    {
        var response = await _adminJobService.EnfileirarAsync(jobId, User.ObterUsuarioId(), cancellationToken);
        if (response is null)
            return NotFound(new MensagemErroResponse { Mensagem = "Job não encontrado." });

        return Ok(response);
    }

    [HttpPost("{jobId}/agendar")]
    [ProducesResponseType(typeof(EnfileirarBackgroundJobAdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Agendar(string jobId, [FromBody] AgendarBackgroundJobAdminRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
            return BadRequest(new MensagemErroResponse { Mensagem = "Requisição inválida." });

        var response = await _adminJobService.AgendarAsync(jobId, request.ProcessarAposUtc, User.ObterUsuarioId(), cancellationToken);
        if (response is null)
            return NotFound(new MensagemErroResponse { Mensagem = "Job não encontrado." });

        return Ok(response);
    }

    [HttpPost("fila/processar")]
    [ProducesResponseType(typeof(ProcessarFilaBackgroundJobAdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ProcessarFila(CancellationToken cancellationToken = default)
    {
        var response = await _adminJobService.ProcessarFilaAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPut("fila/{execucaoId:guid}/cancelar")]
    [ProducesResponseType(typeof(BackgroundJobFilaItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelarExecucao(Guid execucaoId, CancellationToken cancellationToken = default)
    {
        var response = await _adminJobService.CancelarExecucaoAsync(execucaoId, cancellationToken);
        if (response is null)
            return BadRequest(new MensagemErroResponse { Mensagem = "Execução não pode ser cancelada." });

        return Ok(response);
    }

    [HttpPost("{jobId}/cancelar-todos")]
    [ProducesResponseType(typeof(CancelarBackgroundJobAdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelarPorJob(string jobId, CancellationToken cancellationToken = default)
    {
        var response = await _adminJobService.CancelarPorJobAsync(jobId, cancellationToken);
        return Ok(response);
    }

    [HttpPut("fila/{execucaoId:guid}/reabrir")]
    [ProducesResponseType(typeof(BackgroundJobFilaItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ReabrirExecucao(Guid execucaoId, CancellationToken cancellationToken = default)
    {
        var response = await _adminJobService.ReabrirExecucaoAsync(execucaoId, cancellationToken);
        if (response is null)
            return BadRequest(new MensagemErroResponse { Mensagem = "Execução não pode ser reaberta." });

        return Ok(response);
    }

    [HttpGet("fila/metricas")]
    [ProducesResponseType(typeof(BackgroundJobFilaMetricasResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterMetricas(CancellationToken cancellationToken = default)
    {
        var response = await _adminJobService.ObterMetricasAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("fila/alertas")]
    [ProducesResponseType(typeof(IReadOnlyList<BackgroundJobFilaAlertaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterAlertas(CancellationToken cancellationToken = default)
    {
        var response = await _adminJobService.ObterAlertasFilaAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("fila/alertas/historico")]
    [ProducesResponseType(typeof(IReadOnlyList<BackgroundJobFilaAlertasHistoricoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterHistoricoAlertas([FromQuery] int? dias = 7, CancellationToken cancellationToken = default)
    {
        var response = await _adminJobService.ObterHistoricoAlertasAsync(dias ?? 7, cancellationToken);
        return Ok(response);
    }
}
