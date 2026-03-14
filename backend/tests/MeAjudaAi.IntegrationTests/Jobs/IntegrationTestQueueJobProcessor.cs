using System.Threading;
using MeAjudaAi.Application.Interfaces.Jobs;

namespace MeAjudaAi.IntegrationTests.Jobs;

public sealed class IntegrationTestQueueJobProcessor : IBackgroundJobProcessor
{
    private static int _executionCount;

    public const string JobIdConst = "integracao-fila-teste";

    public string JobId => JobIdConst;
    public string Nome => "Job de integração para fila";
    public bool Habilitado => true;
    public int IntervaloSegundos => 30;

    public static int ExecutionCount => Volatile.Read(ref _executionCount);

    public Task<int> ExecutarAsync(CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _executionCount);
        return Task.FromResult(1);
    }

    public static void Reset()
    {
        Interlocked.Exchange(ref _executionCount, 0);
    }
}
