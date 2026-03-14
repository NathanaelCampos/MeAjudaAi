using MeAjudaAi.Application.Interfaces.Jobs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Infrastructure.Services.Jobs;

public abstract class ScheduledBackgroundJobProcessor<TProcessor> : BackgroundService, IBackgroundJobProcessor
    where TProcessor : class
{
    private readonly IBackgroundJobExecutionMetricsService _metricsService;
    private readonly ILogger<TProcessor> _logger;

    protected ScheduledBackgroundJobProcessor(
        IBackgroundJobExecutionMetricsService metricsService,
        ILogger<TProcessor> logger)
    {
        _metricsService = metricsService;
        _logger = logger;
    }

    public abstract string JobId { get; }
    public abstract string Nome { get; }
    public abstract bool Habilitado { get; }
    public abstract int IntervaloSegundos { get; }
    protected abstract int IntervaloMinimoSegundos { get; }
    protected abstract string MensagemDesabilitado { get; }
    protected abstract string MensagemErro { get; }
    protected abstract Task<int> ExecutarInternoAsync(CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _metricsService.RegistrarConfiguracao(JobId, Nome, Habilitado, IntervaloSegundos);

        if (!Habilitado)
        {
            _logger.LogInformation("{Mensagem}", MensagemDesabilitado);
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExecutarComMetricasAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Mensagem}", MensagemErro);
            }

            await Task.Delay(TimeSpan.FromSeconds(Math.Max(IntervaloMinimoSegundos, IntervaloSegundos)), stoppingToken);
        }
    }

    public Task<int> ExecutarAsync(CancellationToken cancellationToken = default)
    {
        return ExecutarComMetricasAsync(cancellationToken);
    }

    private async Task<int> ExecutarComMetricasAsync(CancellationToken cancellationToken)
    {
        var iniciadoEm = DateTime.UtcNow;
        _metricsService.RegistrarInicio(JobId, Nome, Habilitado, IntervaloSegundos, iniciadoEm);

        try
        {
            var processados = await ExecutarInternoAsync(cancellationToken);
            _metricsService.RegistrarSucesso(JobId, Nome, Habilitado, IntervaloSegundos, DateTime.UtcNow, processados);
            return processados;
        }
        catch (Exception ex)
        {
            _metricsService.RegistrarErro(JobId, Nome, Habilitado, IntervaloSegundos, DateTime.UtcNow, ex.Message);
            throw;
        }
    }
}
