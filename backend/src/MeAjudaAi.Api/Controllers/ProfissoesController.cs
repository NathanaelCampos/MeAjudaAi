using MeAjudaAi.Application.DTOs.Profissoes;
using MeAjudaAi.Application.Interfaces.Profissoes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Api.Controllers;

[ApiController]
[Route("api/profissoes")]
public class ProfissoesController : ControllerBase
{
    private readonly IProfissaoService _profissaoService;

    public ProfissoesController(IProfissaoService profissaoService)
    {
        _profissaoService = profissaoService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<ProfissaoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var response = await _profissaoService.ListarAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("{profissaoId:guid}/especialidades")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<EspecialidadeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarEspecialidades(
        Guid profissaoId,
        CancellationToken cancellationToken)
    {
        var response = await _profissaoService.ListarEspecialidadesPorProfissaoAsync(
            profissaoId,
            cancellationToken);

        return Ok(response);
    }
}
