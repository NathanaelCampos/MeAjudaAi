namespace MeAjudaAi.Application.Interfaces.Storage;

public interface IArquivoStorageService
{
    Task<(string NomeArquivo, string CaminhoRelativo)> SalvarArquivoAsync(
        Stream stream,
        string nomeArquivoOriginal,
        string pastaDestino,
        CancellationToken cancellationToken = default);

    Task<(string NomeArquivo, string CaminhoRelativo)> SalvarArquivoProfissionalAsync(
        Stream stream,
        string nomeArquivoOriginal,
        Guid profissionalId,
        string pastaBase,
        CancellationToken cancellationToken = default);

    Task ExcluirArquivoAsync(
        string caminhoRelativo,
        CancellationToken cancellationToken = default);
}