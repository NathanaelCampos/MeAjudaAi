using MeAjudaAi.Api.Extensions;
using MeAjudaAi.Application.DTOs.Avaliacoes;
using MeAjudaAi.Application.Interfaces.Avaliacoes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Api.Controllers;

[ApiController]
[Route("api/avaliacoes")]
public class AvaliacoesController : ControllerBase
{
    private readonly IAvaliacaoService _avaliacaoService;

    public AvaliacoesController(IAvaliacaoService avaliacaoService)
    {
        _avaliacaoService = avaliacaoService;
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(AvaliacaoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Criar(
        [FromBody] CriarAvaliacaoRequest request,
        CancellationToken cancellationToken = default)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        var response = await _avaliacaoService.CriarAsync(
            usuarioId.Value,
            request,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("profissional/{profissionalId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<AvaliacaoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPorProfissional(
        Guid profissionalId,
        CancellationToken cancellationToken = default)
    {
        var response = await _avaliacaoService.ListarPorProfissionalAsync(
            profissionalId,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("pendentes")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(IReadOnlyList<AvaliacaoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPendentes(
        CancellationToken cancellationToken = default)
    {
        var response = await _avaliacaoService.ListarPendentesAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPut("{avaliacaoId:guid}/moderar")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(AvaliacaoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Moderar(
        Guid avaliacaoId,
        [FromBody] ModerarAvaliacaoRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _avaliacaoService.ModerarAsync(
            avaliacaoId,
            request,
            cancellationToken);

        if (response is null)
            return NotFound();

        return Ok(response);
    }
}
