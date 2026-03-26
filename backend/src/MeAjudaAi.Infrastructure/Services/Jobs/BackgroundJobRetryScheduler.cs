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

public class BackgroundJobRetryScheduler : ScheduledBackgroundJobProcessor<BackgroundJobRetryScheduler>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly BackgroundJobRetrySchedulerOptions _options;

    public BackgroundJobRetryScheduler(
        IServiceScopeFactory scopeFactory,
        IOptions<BackgroundJobRetrySchedulerOptions> options,
        IBackgroundJobExecutionMetricsService metricsService,
        ILogger<BackgroundJobRetryScheduler> logger)
        : base(metricsService, logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
    }

    public override string JobId => "retry-scheduler";
    public override string Nome => "Agendador de retry automático";
    public override bool Habilitado => _options.Habilitado;
    public override int IntervaloSegundos => _options.IntervaloSegundos;
    protected override int IntervaloMinimoSegundos => 30;
    protected override string MensagemDesabilitado => "Agendador de retry desabilitado.";
    protected override string MensagemErro => "Erro ao marcar execuções para retry.";

    protected override async Task<int> ExecutarInternoAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var agora = DateTime.UtcNow;
        var limite = agora.AddSeconds(-Math.Max(1, _options.TempoMaximoDesdeFalhaSegundos));

        var falhas = await context.BackgroundJobsExecucoes
            .Where(x => x.Ativo && x.Status == Domain.Enums.StatusExecucaoBackgroundJob.Falha && x.DataFinalizacao <= limite)
            .ToListAsync(cancellationToken);

        int agendadas = 0;
        foreach (var execucao in falhas)
        {
            if (execucao.TentativasProcessamento <= _options.FalhasParaRetentar)
            {
                execucao.Status = Domain.Enums.StatusExecucaoBackgroundJob.Pendente;
                execucao.ProcessarAposUtc = agora;
                execucao.DataInicioProcessamento = null;
                execucao.DataFinalizacao = null;
                execucao.MensagemResultado = "Retry automatizado agendado.";
                execucao.DataAtualizacao = agora;
                agendadas++;
                context.BackgroundJobRetryLogs.Add(CriarLog(execucao, "RetryScheduler", execucao.MensagemResultado));
            }
        }

        if (agendadas > 0)
            await context.SaveChangesAsync(cancellationToken);

        return agendadas;
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
