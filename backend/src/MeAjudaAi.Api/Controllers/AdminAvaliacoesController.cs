using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.Interfaces.Admin;
using MeAjudaAi.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Api.Controllers;

[ApiController]
[Route("api/admin/avaliacoes")]
[Authorize(Roles = "Administrador")]
public class AdminAvaliacoesController : ControllerBase
{
    private readonly IAdminAvaliacaoService _adminAvaliacaoService;

    public AdminAvaliacoesController(IAdminAvaliacaoService adminAvaliacaoService)
    {
        _adminAvaliacaoService = adminAvaliacaoService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginacaoResponse<AvaliacaoAdminListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Buscar(
        [FromQuery] string? termo = null,
        [FromQuery] Guid? clienteId = null,
        [FromQuery] Guid? profissionalId = null,
        [FromQuery] Guid? servicoId = null,
        [FromQuery] StatusModeracaoComentario? statusModeracaoComentario = null,
        [FromQuery] DateTime? dataCriacaoInicial = null,
        [FromQuery] DateTime? dataCriacaoFinal = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _adminAvaliacaoService.BuscarAsync(new BuscarAvaliacoesAdminRequest
        {
            Termo = termo,
            ClienteId = clienteId,
            ProfissionalId = profissionalId,
            ServicoId = servicoId,
            StatusModeracaoComentario = statusModeracaoComentario,
            DataCriacaoInicial = dataCriacaoInicial,
            DataCriacaoFinal = dataCriacaoFinal,
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina
        }, cancellationToken);

        return Ok(response);
    }

    [HttpGet("{avaliacaoId:guid}")]
    [ProducesResponseType(typeof(AvaliacaoAdminDetalheResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterPorId(
        Guid avaliacaoId,
        CancellationToken cancellationToken = default)
    {
        var response = await _adminAvaliacaoService.ObterPorIdAsync(avaliacaoId, cancellationToken);

        if (response is null)
            return NotFound(new MensagemErroResponse { Mensagem = "Avaliação não encontrada." });

        return Ok(response);
    }
}
