namespace MeAjudaAi.Application.Interfaces.Jobs;

public interface IBackgroundJobProcessor
{
    string JobId { get; }
    string Nome { get; }
    bool Habilitado { get; }
    int IntervaloSegundos { get; }
    Task<int> ExecutarAsync(CancellationToken cancellationToken = default);
}
