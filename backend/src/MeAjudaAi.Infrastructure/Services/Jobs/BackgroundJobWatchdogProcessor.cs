using MeAjudaAi.Application.Interfaces.Jobs;
using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Infrastructure.Configurations;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Infrastructure.Services.Jobs;

public class BackgroundJobWatchdogProcessor : ScheduledBackgroundJobProcessor<BackgroundJobWatchdogProcessor>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly BackgroundJobWatchdogOptions _options;

    public BackgroundJobWatchdogProcessor(
        IServiceScopeFactory scopeFactory,
        IOptions<BackgroundJobWatchdogOptions> options,
        IBackgroundJobExecutionMetricsService metricsService,
        ILogger<BackgroundJobWatchdogProcessor> logger)
        : base(metricsService, logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
    }

    public override string JobId => "watchdog-job-fila";
    public override string Nome => "Watchdog de execução de jobs";
    public override bool Habilitado => _options.Habilitado;
    public override int IntervaloSegundos => _options.IntervaloSegundos;
    protected override int IntervaloMinimoSegundos => 30;
    protected override string MensagemDesabilitado => "Watchdog de jobs desabilitado por configuração.";
    protected override string MensagemErro => "Erro ao inspecionar execuções travadas.";

    protected override async Task<int> ExecutarInternoAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var limite = DateTime.UtcNow.AddSeconds(-Math.Max(1, _options.TempoMaximoProcessandoSegundos));

        var travadas = await context.BackgroundJobsExecucoes
            .Where(x => x.Ativo && x.Status == StatusExecucaoBackgroundJob.Processando && x.DataInicioProcessamento <= limite)
            .ToListAsync(cancellationToken);

        if (travadas.Count == 0)
            return 0;

        foreach (var execucao in travadas)
        {
            execucao.Status = StatusExecucaoBackgroundJob.Pendente;
            execucao.ProcessarAposUtc = DateTime.UtcNow;
            execucao.DataInicioProcessamento = null;
            execucao.DataFinalizacao = null;
            execucao.MensagemResultado = "Execução reiniciada pelo watchdog (tempo excedido).";
            execucao.DataAtualizacao = DateTime.UtcNow;
            context.BackgroundJobRetryLogs.Add(CriarLog(execucao, "Watchdog", execucao.MensagemResultado));
        }

        await context.SaveChangesAsync(cancellationToken);
        return travadas.Count;
    }

    private static BackgroundJobRetryLog CriarLog(BackgroundJobExecucao execucao, string tipo, string mensagem)
    {
        return new BackgroundJobRetryLog
        {
            Id = Guid.NewGuid(),
            BackgroundJobExecucaoId = execucao.Id,
            JobId = execucao.JobId,
            Tipo = tipo,
            Mensagem = mensagem,
            DataCriacao = DateTime.UtcNow,
            Ativo = true
        };
    }
}
