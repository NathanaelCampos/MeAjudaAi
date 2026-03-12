using MeAjudaAi.Application.Interfaces.Storage;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.Infrastructure.Services.Storage;

public class ArquivoStorageService : IArquivoStorageService
{
    private readonly IHostEnvironment _environment;

    public ArquivoStorageService(IHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<(string NomeArquivo, string CaminhoRelativo)> SalvarArquivoAsync(
        Stream stream,
        string nomeArquivoOriginal,
        string pastaDestino,
        CancellationToken cancellationToken = default)
    {
        var extensao = Path.GetExtension(nomeArquivoOriginal);
        var nomeArquivo = $"{Guid.NewGuid()}{extensao}";

        var caminhoBaseUploads = Path.Combine(_environment.ContentRootPath, "Uploads");
        var caminhoPastaDestino = Path.Combine(caminhoBaseUploads, pastaDestino);

        if (!Directory.Exists(caminhoPastaDestino))
            Directory.CreateDirectory(caminhoPastaDestino);

        var caminhoCompleto = Path.Combine(caminhoPastaDestino, nomeArquivo);

        await using var fileStream = new FileStream(caminhoCompleto, FileMode.Create);
        await stream.CopyToAsync(fileStream, cancellationToken);

        var caminhoRelativo = $"/uploads/{pastaDestino}/{nomeArquivo}".Replace("\\", "/");

        return (nomeArquivo, caminhoRelativo);
    }

    public async Task<(string NomeArquivo, string CaminhoRelativo)> SalvarArquivoProfissionalAsync(
        Stream stream,
        string nomeArquivoOriginal,
        Guid profissionalId,
        string pastaBase,
        CancellationToken cancellationToken = default)
    {
        var pastaDestino = Path.Combine(pastaBase, profissionalId.ToString());
        return await SalvarArquivoAsync(stream, nomeArquivoOriginal, pastaDestino, cancellationToken);
    }

    public Task ExcluirArquivoAsync(
        string caminhoRelativo,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(caminhoRelativo))
            return Task.CompletedTask;

        var caminhoNormalizado = caminhoRelativo.Trim();

        if (Uri.TryCreate(caminhoNormalizado, UriKind.Absolute, out var uri))
        {
            caminhoNormalizado = uri.AbsolutePath;
        }

        caminhoNormalizado = caminhoNormalizado.Replace("/uploads/", string.Empty).TrimStart('/');
        caminhoNormalizado = caminhoNormalizado.Replace("/", Path.DirectorySeparatorChar.ToString());

        var caminhoBaseUploads = Path.Combine(_environment.ContentRootPath, "Uploads");
        var caminhoCompleto = Path.Combine(caminhoBaseUploads, caminhoNormalizado);

        if (File.Exists(caminhoCompleto))
        {
            File.Delete(caminhoCompleto);
        }

        return Task.CompletedTask;
    }
}