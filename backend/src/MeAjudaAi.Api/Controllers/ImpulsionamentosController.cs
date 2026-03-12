using MeAjudaAi.Api.Extensions;
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

    public ImpulsionamentosController(IImpulsionamentoService impulsionamentoService)
    {
        _impulsionamentoService = impulsionamentoService;
    }

    [HttpGet("planos")]
    [AllowAnonymous]
    public async Task<IActionResult> ListarPlanos(CancellationToken cancellationToken = default)
    {
        var response = await _impulsionamentoService.ListarPlanosAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPost("contratar")]
    [Authorize]
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
}
