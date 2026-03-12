using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Infrastructure.Importacao.Modelos;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Infrastructure.Importacao;

public class ImportadorGeografiaService
{
    private readonly AppDbContext _context;

    public ImportadorGeografiaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task ImportarEstadosAsync(string caminhoArquivo, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(caminhoArquivo))
            throw new FileNotFoundException("Arquivo de estados não encontrado.", caminhoArquivo);

        using var reader = new StreamReader(caminhoArquivo);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
            PrepareHeaderForMatch = args => args.Header.Trim().ToLowerInvariant()
        });

        var registros = csv.GetRecords<EstadoCsvModel>().ToList();

        foreach (var item in registros)
        {
            if (string.IsNullOrWhiteSpace(item.codigo_ibge) ||
                string.IsNullOrWhiteSpace(item.uf) ||
                string.IsNullOrWhiteSpace(item.nome))
                continue;

            var existe = await _context.Estados
                .AnyAsync(x => x.CodigoIbge == item.codigo_ibge, cancellationToken);

            if (existe)
                continue;

            _context.Estados.Add(new Estado
            {
                CodigoIbge = item.codigo_ibge.Trim(),
                UF = item.uf.Trim(),
                Nome = item.nome.Trim()
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ImportarCidadesAsync(string caminhoArquivo, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(caminhoArquivo))
            throw new FileNotFoundException("Arquivo de municípios não encontrado.", caminhoArquivo);

        using var reader = new StreamReader(caminhoArquivo);
        using var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
            PrepareHeaderForMatch = args => args.Header.Trim().ToLowerInvariant()
        });

        var registros = csv.GetRecords<MeAjudaAi.Infrastructure.Importacao.Modelos.MunicipioCsvModel>().ToList();

        var processados = 0;
        var ignorados = 0;
        var semEstado = 0;

        foreach (var item in registros)
        {
            var codigoIbge = item.codigo_ibge?.Trim();
            var nome = item.nome?.Trim();
            var codigoUf = item.codigo_uf?.Trim();
            var uf = item.uf?.Trim();

            if (string.IsNullOrWhiteSpace(codigoIbge) || string.IsNullOrWhiteSpace(nome))
            {
                ignorados++;
                continue;
            }

            MeAjudaAi.Domain.Entities.Estado? estado = null;

            if (!string.IsNullOrWhiteSpace(codigoUf))
            {
                estado = await _context.Estados
                    .FirstOrDefaultAsync(x => x.CodigoIbge == codigoUf, cancellationToken);
            }

            if (estado is null && !string.IsNullOrWhiteSpace(uf))
            {
                estado = await _context.Estados
                    .FirstOrDefaultAsync(x => x.UF == uf, cancellationToken);
            }

            if (estado is null && codigoIbge.Length >= 2)
            {
                var prefixoUf = codigoIbge.Substring(0, 2);

                estado = await _context.Estados
                    .FirstOrDefaultAsync(x => x.CodigoIbge == prefixoUf, cancellationToken);
            }

            if (estado is null)
            {
                semEstado++;
                continue;
            }

            var existe = await _context.Cidades
                .AnyAsync(x => x.CodigoIbge == codigoIbge, cancellationToken);

            if (existe)
                continue;

            _context.Cidades.Add(new Cidade
            {
                EstadoId = estado.Id,
                CodigoIbge = codigoIbge,
                Nome = nome
            });

            processados++;
        }

        await _context.SaveChangesAsync(cancellationToken);

        Console.WriteLine($"Cidades importadas: {processados}");
        Console.WriteLine($"Cidades ignoradas: {ignorados}");
        Console.WriteLine($"Cidades sem estado correspondente: {semEstado}");
    }

    public async Task ImportarBairrosAsync(string caminhoArquivo, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(caminhoArquivo))
            throw new FileNotFoundException("Arquivo de bairros não encontrado.", caminhoArquivo);

        using var reader = new StreamReader(caminhoArquivo);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HeaderValidated = null,
            MissingFieldFound = null,
            PrepareHeaderForMatch = args => args.Header.Trim().ToLowerInvariant().Replace("\"", "")
        });

        var registros = csv.GetRecords<BairroCsvModel>().ToList();

        foreach (var item in registros)
        {
            var uf = item.uf?.Trim();
            var nomeCidade = item.municipio?.Trim();
            var nomeBairro = item.bairro?.Trim();

            if (string.IsNullOrWhiteSpace(uf) ||
                string.IsNullOrWhiteSpace(nomeCidade) ||
                string.IsNullOrWhiteSpace(nomeBairro))
                continue;

            var cidade = await _context.Cidades
                .Include(x => x.Estado)
                .FirstOrDefaultAsync(
                    x => x.Nome == nomeCidade && x.Estado.UF == uf,
                    cancellationToken);

            if (cidade is null)
                continue;

            var existe = await _context.Bairros
                .AnyAsync(x => x.CidadeId == cidade.Id && x.Nome == nomeBairro, cancellationToken);

            if (existe)
                continue;

            _context.Bairros.Add(new Bairro
            {
                CidadeId = cidade.Id,
                Nome = nomeBairro
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}