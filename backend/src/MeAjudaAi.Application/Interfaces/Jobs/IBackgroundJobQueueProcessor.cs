namespace MeAjudaAi.Application.Interfaces.Jobs;

public interface IBackgroundJobQueueProcessor
{
    Task<int> ProcessarPendentesAsync(CancellationToken cancellationToken = default);
}
