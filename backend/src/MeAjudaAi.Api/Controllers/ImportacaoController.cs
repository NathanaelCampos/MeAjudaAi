using MeAjudaAi.Application.DTOs.Cidades;
using MeAjudaAi.Infrastructure.Importacao;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Api.Controllers;

[ApiController]
[Route("api/importacao")]
[Authorize(Roles = "Administrador")]
public class ImportacaoController : ControllerBase
{
    private readonly ImportadorGeografiaService _importadorGeografiaService;
    private readonly IWebHostEnvironment _environment;
    private readonly AppDbContext _context;

    public ImportacaoController(
        ImportadorGeografiaService importadorGeografiaService,
        IWebHostEnvironment environment,
        AppDbContext context)
    {
        _importadorGeografiaService = importadorGeografiaService;
        _environment = environment;
        _context = context;
    }

    [HttpPost("geografia")]
    [ProducesResponseType(typeof(ImportacaoGeografiaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ImportarGeografia(CancellationToken cancellationToken)
    {
        var basePath = Path.Combine(_environment.ContentRootPath, "DadosCsv");

        var caminhoEstados = Path.Combine(basePath, "estados.csv");
        var caminhoCidades = Path.Combine(basePath, "municipios.csv");
        var caminhoBairros = Path.Combine(basePath, "bairros.csv");

        await _importadorGeografiaService.ImportarEstadosAsync(caminhoEstados, cancellationToken);
        await _importadorGeografiaService.ImportarCidadesAsync(caminhoCidades, cancellationToken);
        await _importadorGeografiaService.ImportarBairrosAsync(caminhoBairros, cancellationToken);

        return Ok(new ImportacaoGeografiaResponse
        {
            Mensagem = "Importação concluída.",
            Estados = _context.Estados.Count(),
            Cidades = _context.Cidades.Count(),
            Bairros = _context.Bairros.Count()
        });
    }
}
