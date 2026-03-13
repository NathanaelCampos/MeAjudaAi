using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.Interfaces.Admin;
using MeAjudaAi.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Api.Controllers;

[ApiController]
[Route("api/admin/profissionais")]
[Authorize(Roles = "Administrador")]
public class AdminProfissionaisController : ControllerBase
{
    private readonly IAdminProfissionalService _adminProfissionalService;

    public AdminProfissionaisController(IAdminProfissionalService adminProfissionalService)
    {
        _adminProfissionalService = adminProfissionalService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginacaoResponse<ProfissionalAdminListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Buscar(
        [FromQuery] string? nome = null,
        [FromQuery] bool? ativo = null,
        [FromQuery] bool? perfilVerificado = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _adminProfissionalService.BuscarAsync(new BuscarProfissionaisAdminRequest
        {
            Nome = nome,
            Ativo = ativo,
            PerfilVerificado = perfilVerificado,
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina
        }, cancellationToken);

        return Ok(response);
    }

    [HttpGet("{profissionalId:guid}")]
    [ProducesResponseType(typeof(ProfissionalAdminDetalheResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterPorId(
        Guid profissionalId,
        CancellationToken cancellationToken = default)
    {
        var response = await _adminProfissionalService.ObterPorIdAsync(profissionalId, cancellationToken);

        if (response is null)
            return NotFound(new MensagemErroResponse { Mensagem = "Profissional não encontrado." });

        return Ok(response);
    }

    [HttpGet("{profissionalId:guid}/dashboard")]
    [ProducesResponseType(typeof(ProfissionalAdminDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterDashboard(
        Guid profissionalId,
        CancellationToken cancellationToken = default)
    {
        var response = await _adminProfissionalService.ObterDashboardAsync(profissionalId, cancellationToken);

        if (response is null)
            return NotFound(new MensagemErroResponse { Mensagem = "Profissional não encontrado." });

        return Ok(response);
    }

    [HttpPut("{profissionalId:guid}/verificar")]
    [ProducesResponseType(typeof(ProfissionalAdminDetalheResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Verificar(
        Guid profissionalId,
        CancellationToken cancellationToken = default)
    {
        var adminId = User.ObterUsuarioId();
        var response = await _adminProfissionalService.DefinirPerfilVerificadoAsync(profissionalId, true, adminId, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{profissionalId:guid}/desverificar")]
    [ProducesResponseType(typeof(ProfissionalAdminDetalheResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Desverificar(
        Guid profissionalId,
        CancellationToken cancellationToken = default)
    {
        var adminId = User.ObterUsuarioId();
        var response = await _adminProfissionalService.DefinirPerfilVerificadoAsync(profissionalId, false, adminId, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{profissionalId:guid}/ativar")]
    [ProducesResponseType(typeof(ProfissionalAdminDetalheResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Ativar(
        Guid profissionalId,
        CancellationToken cancellationToken = default)
    {
        var adminId = User.ObterUsuarioId();
        var response = await _adminProfissionalService.DefinirAtivoAsync(profissionalId, true, adminId, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{profissionalId:guid}/desativar")]
    [ProducesResponseType(typeof(ProfissionalAdminDetalheResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Desativar(
        Guid profissionalId,
        CancellationToken cancellationToken = default)
    {
        var adminId = User.ObterUsuarioId();
        var response = await _adminProfissionalService.DefinirAtivoAsync(profissionalId, false, adminId, cancellationToken);
        return Ok(response);
    }
}
