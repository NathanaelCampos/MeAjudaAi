using MeAjudaAi.Api.Extensions;
using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.Interfaces.Admin;
using MeAjudaAi.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Api.Controllers;

[ApiController]
[Route("api/admin/usuarios")]
[Authorize(Roles = "Administrador")]
public class AdminUsuariosController : ControllerBase
{
    private readonly IAdminUsuarioService _adminUsuarioService;

    public AdminUsuariosController(IAdminUsuarioService adminUsuarioService)
    {
        _adminUsuarioService = adminUsuarioService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginacaoResponse<UsuarioAdminListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Buscar(
        [FromQuery] string? nome = null,
        [FromQuery] string? email = null,
        [FromQuery] TipoPerfil? tipoPerfil = null,
        [FromQuery] bool? ativo = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _adminUsuarioService.BuscarAsync(new BuscarUsuariosAdminRequest
        {
            Nome = nome,
            Email = email,
            TipoPerfil = tipoPerfil,
            Ativo = ativo,
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina
        }, cancellationToken);

        return Ok(response);
    }

    [HttpGet("{usuarioId:guid}")]
    [ProducesResponseType(typeof(UsuarioAdminDetalheResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterPorId(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        var response = await _adminUsuarioService.ObterPorIdAsync(usuarioId, cancellationToken);

        if (response is null)
            return NotFound(new MensagemErroResponse { Mensagem = "Usuário não encontrado." });

        return Ok(response);
    }

    [HttpPut("{usuarioId:guid}/bloquear")]
    [ProducesResponseType(typeof(UsuarioAdminDetalheResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Bloquear(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        var adminId = User.ObterUsuarioId();
        var response = await _adminUsuarioService.DefinirAtivoAsync(usuarioId, false, adminId, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{usuarioId:guid}/desbloquear")]
    [ProducesResponseType(typeof(UsuarioAdminDetalheResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Desbloquear(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        var adminId = User.ObterUsuarioId();
        var response = await _adminUsuarioService.DefinirAtivoAsync(usuarioId, true, adminId, cancellationToken);
        return Ok(response);
    }
}
