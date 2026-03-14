using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.Interfaces.Admin;
using MeAjudaAi.Application.Interfaces.Jobs;

namespace MeAjudaAi.Infrastructure.Services.Admin;

public class AdminJobService : IAdminJobService
{
    private readonly IEnumerable<IBackgroundJobProcessor> _jobs;
    private readonly IBackgroundJobExecutionMetricsService _metricsService;

    public AdminJobService(
        IEnumerable<IBackgroundJobProcessor> jobs,
        IBackgroundJobExecutionMetricsService metricsService)
    {
        _jobs = jobs;
        _metricsService = metricsService;
    }

    public Task<IReadOnlyList<BackgroundJobAdminItemResponse>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var itens = _jobs
            .OrderBy(x => x.Nome)
            .Select(x => _metricsService.ObterSnapshot(x.JobId, x.Nome, x.Habilitado, x.IntervaloSegundos))
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyList<BackgroundJobAdminItemResponse>>(itens);
    }

    public async Task<ExecutarBackgroundJobAdminResponse?> ExecutarAsync(string jobId, CancellationToken cancellationToken = default)
    {
        var job = _jobs.FirstOrDefault(x => string.Equals(x.JobId, jobId, StringComparison.OrdinalIgnoreCase));
        if (job is null)
            return null;

        var processados = await job.ExecutarAsync(cancellationToken);

        return new ExecutarBackgroundJobAdminResponse
        {
            JobId = job.JobId,
            Nome = job.Nome,
            RegistrosProcessados = processados,
            ExecutadoEm = DateTime.UtcNow
        };
    }
}
