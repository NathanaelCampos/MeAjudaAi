using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Api.Controllers;

[ApiController]
[Route("api/admin/auditoria")]
[Authorize(Roles = "Administrador")]
public class AdminAuditoriaController : ControllerBase
{
    private readonly IAdminAuditoriaService _adminAuditoriaService;

    public AdminAuditoriaController(IAdminAuditoriaService adminAuditoriaService)
    {
        _adminAuditoriaService = adminAuditoriaService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginacaoResponse<AuditoriaAdminListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Buscar(
        [FromQuery] Guid? adminUsuarioId = null,
        [FromQuery] string? entidade = null,
        [FromQuery] Guid? entidadeId = null,
        [FromQuery] string? acao = null,
        [FromQuery] DateTime? dataCriacaoInicial = null,
        [FromQuery] DateTime? dataCriacaoFinal = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _adminAuditoriaService.BuscarAsync(new BuscarAuditoriasAdminRequest
        {
            AdminUsuarioId = adminUsuarioId,
            Entidade = entidade,
            EntidadeId = entidadeId,
            Acao = acao,
            DataCriacaoInicial = dataCriacaoInicial,
            DataCriacaoFinal = dataCriacaoFinal,
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina
        }, cancellationToken);

        return Ok(response);
    }

    [HttpGet("{auditoriaId:guid}")]
    [ProducesResponseType(typeof(AuditoriaAdminDetalheResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterPorId(
        Guid auditoriaId,
        CancellationToken cancellationToken = default)
    {
        var response = await _adminAuditoriaService.ObterPorIdAsync(auditoriaId, cancellationToken);

        if (response is null)
            return NotFound(new MensagemErroResponse { Mensagem = "Auditoria não encontrada." });

        return Ok(response);
    }
}
