using MeAjudaAi.Api.Extensions;
using MeAjudaAi.Application.DTOs.Servicos;
using MeAjudaAi.Application.Interfaces.Servicos;
using MeAjudaAi.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Api.Controllers;

[ApiController]
[Route("api/servicos")]
public class ServicosController : ControllerBase
{
    private readonly IServicoService _servicoService;

    public ServicosController(IServicoService servicoService)
    {
        _servicoService = servicoService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Criar(
        [FromBody] CriarServicoRequest request,
        CancellationToken cancellationToken = default)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        var response = await _servicoService.CriarAsync(
            usuarioId.Value,
            request,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("{servicoId:guid}")]
    [Authorize]
    public async Task<IActionResult> ObterPorId(
        Guid servicoId,
        CancellationToken cancellationToken = default)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        var response = await _servicoService.ObterPorIdAsync(
            usuarioId.Value,
            servicoId,
            cancellationToken);

        if (response is null)
            return NotFound();

        return Ok(response);
    }

    [HttpPut("{servicoId:guid}/aceitar")]
    [Authorize]
    public async Task<IActionResult> Aceitar(
        Guid servicoId,
        CancellationToken cancellationToken = default)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        var response = await _servicoService.AceitarAsync(
            usuarioId.Value,
            servicoId,
            cancellationToken);

        if (response is null)
            return NotFound();

        return Ok(response);
    }

    [HttpPut("{servicoId:guid}/iniciar")]
    [Authorize]
    public async Task<IActionResult> Iniciar(
        Guid servicoId,
        CancellationToken cancellationToken = default)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        var response = await _servicoService.IniciarAsync(
            usuarioId.Value,
            servicoId,
            cancellationToken);

        if (response is null)
            return NotFound();

        return Ok(response);
    }

    [HttpPut("{servicoId:guid}/concluir")]
    [Authorize]
    public async Task<IActionResult> Concluir(
        Guid servicoId,
        CancellationToken cancellationToken = default)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        var response = await _servicoService.ConcluirAsync(
            usuarioId.Value,
            servicoId,
            cancellationToken);

        if (response is null)
            return NotFound();

        return Ok(response);
    }

    [HttpPut("{servicoId:guid}/cancelar")]
    [Authorize]
    public async Task<IActionResult> Cancelar(
        Guid servicoId,
        CancellationToken cancellationToken = default)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        var response = await _servicoService.CancelarAsync(
            usuarioId.Value,
            servicoId,
            cancellationToken);

        if (response is null)
            return NotFound();

        return Ok(response);
    }

    [HttpGet("me/cliente")]
    [Authorize]
    public async Task<IActionResult> ListarMeusServicosCliente(
        [FromQuery] StatusServico? status,
        CancellationToken cancellationToken = default)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        var response = await _servicoService.ListarMeusServicosClienteAsync(
            usuarioId.Value,
            new ListarServicosRequest { Status = status },
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("me/profissional")]
    [Authorize]
    public async Task<IActionResult> ListarMeusServicosProfissional(
        [FromQuery] StatusServico? status,
        CancellationToken cancellationToken = default)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        var response = await _servicoService.ListarMeusServicosProfissionalAsync(
            usuarioId.Value,
            new ListarServicosRequest { Status = status },
            cancellationToken);

        return Ok(response);
    }
}
