using MeAjudaAi.Application.DTOs.Cidades;
using MeAjudaAi.Application.Interfaces.Cidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Api.Controllers;

[ApiController]
[Route("api/cidades")]
public class CidadesController : ControllerBase
{
    private readonly ICidadeService _cidadeService;

    public CidadesController(ICidadeService cidadeService)
    {
        _cidadeService = cidadeService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<CidadeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var response = await _cidadeService.ListarAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("{cidadeId:guid}/bairros")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<BairroResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarBairros(
        Guid cidadeId,
        CancellationToken cancellationToken)
    {
        var response = await _cidadeService.ListarBairrosPorCidadeAsync(
            cidadeId,
            cancellationToken);

        return Ok(response);
    }
}
