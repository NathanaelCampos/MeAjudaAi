using MeAjudaAi.Api.Extensions;
using MeAjudaAi.Application.DTOs.Profissionais;
using MeAjudaAi.Application.Interfaces.Profissionais;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeAjudaAi.Application.Interfaces.Storage;

namespace MeAjudaAi.Api.Controllers;

[ApiController]
[Route("api/profissionais")]
public class ProfissionaisController : ControllerBase
{
    private readonly IProfissionalService _profissionalService;
    private readonly IArquivoStorageService _arquivoStorageService;

    public ProfissionaisController(
    IProfissionalService profissionalService,
    IArquivoStorageService arquivoStorageService)
    {
        _profissionalService = profissionalService;
        _arquivoStorageService = arquivoStorageService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<ProfissionalResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? nome,
        [FromQuery] bool somenteAtivos = true,
        CancellationToken cancellationToken = default)
    {
        var request = new ListarProfissionaisRequest
        {
            Nome = nome,
            SomenteAtivos = somenteAtivos
        };

        var response = await _profissionalService.ListarAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProfissionalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var response = await _profissionalService.ObterPorIdAsync(id, cancellationToken);

        if (response is null)
            return NotFound();

        return Ok(response);
    }

    [HttpPut("me")]
    [Authorize]
    [ProducesResponseType(typeof(ProfissionalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarMeuPerfil(
        [FromBody] AtualizarProfissionalRequest request,
        CancellationToken cancellationToken = default)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        var response = await _profissionalService.AtualizarPorUsuarioIdAsync(
            usuarioId.Value,
            request,
            cancellationToken);

        if (response is null)
            return NotFound(new { mensagem = "Perfil profissional não encontrado para o usuário autenticado." });

        return Ok(response);
    }
    [HttpPut("me/profissoes")]
    [Authorize]
    public async Task<IActionResult> AtualizarProfissoes(
    [FromBody] AtualizarProfissoesProfissionalRequest request,
    CancellationToken cancellationToken = default)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        try
        {
            await _profissionalService.AtualizarProfissoesAsync(
                usuarioId.Value,
                request,
                cancellationToken);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
    }

    [HttpPut("me/especialidades")]
    [Authorize]
    public async Task<IActionResult> AtualizarEspecialidades(
        [FromBody] AtualizarEspecialidadesProfissionalRequest request,
        CancellationToken cancellationToken = default)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        try
        {
            await _profissionalService.AtualizarEspecialidadesAsync(
                usuarioId.Value,
                request,
                cancellationToken);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
    }

    [HttpPut("me/areas-atendimento")]
    [Authorize]
    public async Task<IActionResult> AtualizarAreasAtendimento(
    [FromBody] AtualizarAreasAtendimentoRequest request,
    CancellationToken cancellationToken = default)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        try
        {
            await _profissionalService.AtualizarAreasAtendimentoAsync(
                usuarioId.Value,
                request,
                cancellationToken);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
    }

    [HttpGet("buscar")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Buscar(
     [FromQuery] string? nome,
     [FromQuery] Guid? profissaoId,
     [FromQuery] Guid? especialidadeId,
     [FromQuery] Guid? cidadeId,
     [FromQuery] Guid? bairroId,
     [FromQuery] bool somenteAtivos = true,
     [FromQuery] decimal? notaMinimaServico = null,
     [FromQuery] decimal? notaMinimaAtendimento = null,
     [FromQuery] decimal? notaMinimaPreco = null,
     [FromQuery] OrdenacaoProfissionais ordenacao = OrdenacaoProfissionais.Relevancia,
     [FromQuery] int pagina = 1,
     [FromQuery] int tamanhoPagina = 10,
     CancellationToken cancellationToken = default)
    {
        var request = new BuscarProfissionaisRequest
        {
            Nome = nome,
            ProfissaoId = profissaoId,
            EspecialidadeId = especialidadeId,
            CidadeId = cidadeId,
            BairroId = bairroId,
            SomenteAtivos = somenteAtivos,
            NotaMinimaServico = notaMinimaServico,
            NotaMinimaAtendimento = notaMinimaAtendimento,
            NotaMinimaPreco = notaMinimaPreco,
            Ordenacao = ordenacao,
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina
        };

        var response = await _profissionalService.BuscarAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}/portfolio")]
    [AllowAnonymous]
    public async Task<IActionResult> ListarPortfolio(
    Guid id,
    CancellationToken cancellationToken = default)
    {
        var response = await _profissionalService.ListarPortfolioAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpPut("me/portfolio")]
    [Authorize]
    public async Task<IActionResult> AtualizarPortfolio(
        [FromBody] AtualizarPortfolioRequest request,
        CancellationToken cancellationToken = default)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        try
        {
            await _profissionalService.AtualizarPortfolioAsync(
                usuarioId.Value,
                request,
                cancellationToken);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
    }

    [HttpGet("{id:guid}/formas-recebimento")]
    [AllowAnonymous]
    public async Task<IActionResult> ListarFormasRecebimento(
    Guid id,
    CancellationToken cancellationToken = default)
    {
        var response = await _profissionalService.ListarFormasRecebimentoAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpPut("me/formas-recebimento")]
    [Authorize]
    public async Task<IActionResult> AtualizarFormasRecebimento(
        [FromBody] AtualizarFormasRecebimentoRequest request,
        CancellationToken cancellationToken = default)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        try
        {
            await _profissionalService.AtualizarFormasRecebimentoAsync(
                usuarioId.Value,
                request,
                cancellationToken);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
    }

    [HttpGet("{id:guid}/detalhes")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProfissionalDetalhesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterDetalhesPorId(
    Guid id,
    CancellationToken cancellationToken = default)
    {
        var response = await _profissionalService.ObterDetalhesPorIdAsync(id, cancellationToken);

        if (response is null)
            return NotFound();

        return Ok(response);
    }

    [HttpPost("me/upload-portfolio")]
    [Authorize]
    [RequestSizeLimit(10_000_000)]
    [ProducesResponseType(typeof(UploadPortfolioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadPortfolio(
    IFormFile arquivo,
    CancellationToken cancellationToken = default)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        if (arquivo is null || arquivo.Length == 0)
            return BadRequest(new { mensagem = "Arquivo é obrigatório." });

        try
        {
            await using var stream = arquivo.OpenReadStream();

            var response = await _profissionalService.UploadPortfolioAsync(
                usuarioId.Value,
                stream,
                arquivo.FileName,
                arquivo.ContentType,
                arquivo.Length,
                cancellationToken);

            var urlBase = $"{Request.Scheme}://{Request.Host}";
            response.UrlArquivo = $"{urlBase}{response.UrlArquivo}";

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }
}
