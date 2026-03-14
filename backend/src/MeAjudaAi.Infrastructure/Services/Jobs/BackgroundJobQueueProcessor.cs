using MeAjudaAi.Application.Interfaces.Jobs;
using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Infrastructure.Configurations;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Infrastructure.Services.Jobs;

public class BackgroundJobQueueProcessor : BackgroundService, IBackgroundJobQueueProcessor
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEnumerable<IBackgroundJobProcessor> _jobs;
    private readonly IOptions<BackgroundJobQueueOptions> _options;
    private readonly ILogger<BackgroundJobQueueProcessor> _logger;

    public BackgroundJobQueueProcessor(
        IServiceScopeFactory scopeFactory,
        IEnumerable<IBackgroundJobProcessor> jobs,
        IOptions<BackgroundJobQueueOptions> options,
        ILogger<BackgroundJobQueueProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _jobs = jobs;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Value.Habilitada)
        {
            _logger.LogInformation("Processador da fila de jobs desabilitado por configuração.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessarPendentesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar fila persistida de jobs.");
            }

            await Task.Delay(TimeSpan.FromSeconds(Math.Max(5, _options.Value.IntervaloSegundos)), stoppingToken);
        }
    }

    public async Task<int> ProcessarPendentesAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var agora = DateTime.UtcNow;
        var lote = Math.Max(1, _options.Value.LoteProcessamento);

        var pendentes = await context.Set<BackgroundJobExecucao>()
            .Where(x =>
                x.Ativo &&
                x.Status == StatusExecucaoBackgroundJob.Pendente &&
                (x.ProcessarAposUtc == null || x.ProcessarAposUtc <= agora))
            .OrderBy(x => x.DataCriacao)
            .Take(lote)
            .ToListAsync(cancellationToken);

        if (pendentes.Count == 0)
            return 0;

        foreach (var execucao in pendentes)
        {
            execucao.Status = StatusExecucaoBackgroundJob.Processando;
            execucao.DataInicioProcessamento = DateTime.UtcNow;
            execucao.TentativasProcessamento++;
            execucao.DataAtualizacao = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);

        foreach (var execucao in pendentes)
        {
            var job = _jobs.FirstOrDefault(x => string.Equals(x.JobId, execucao.JobId, StringComparison.OrdinalIgnoreCase));
            if (job is null)
            {
                execucao.Status = StatusExecucaoBackgroundJob.Cancelado;
                execucao.DataFinalizacao = DateTime.UtcNow;
                execucao.MensagemResultado = "Job não registrado na aplicação.";
                execucao.DataAtualizacao = DateTime.UtcNow;
                continue;
            }

            try
            {
                var processados = await job.ExecutarAsync(cancellationToken);
                execucao.Status = StatusExecucaoBackgroundJob.Sucesso;
                execucao.RegistrosProcessados = processados;
                execucao.DataFinalizacao = DateTime.UtcNow;
                execucao.MensagemResultado = "Execução concluída com sucesso.";
                execucao.DataAtualizacao = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                if (execucao.TentativasProcessamento >= Math.Max(1, _options.Value.MaxTentativas))
                {
                    execucao.Status = StatusExecucaoBackgroundJob.Cancelado;
                    execucao.ProcessarAposUtc = null;
                    execucao.MensagemResultado = $"Execução cancelada após atingir o limite de tentativas. Último erro: {ex.Message}";
                }
                else
                {
                    execucao.Status = StatusExecucaoBackgroundJob.Falha;
                    execucao.ProcessarAposUtc = DateTime.UtcNow.AddSeconds(Math.Max(5, _options.Value.AtrasoBaseSegundos) * execucao.TentativasProcessamento);
                    execucao.MensagemResultado = ex.Message;
                }

                execucao.DataFinalizacao = DateTime.UtcNow;
                execucao.DataAtualizacao = DateTime.UtcNow;
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return pendentes.Count;
    }
}
