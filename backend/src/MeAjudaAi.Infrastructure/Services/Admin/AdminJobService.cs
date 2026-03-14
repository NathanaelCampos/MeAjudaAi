using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.Interfaces.Admin;
using MeAjudaAi.Application.Interfaces.Jobs;
using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Infrastructure.Services.Admin;

public class AdminJobService : IAdminJobService
{
    private readonly IEnumerable<IBackgroundJobProcessor> _jobs;
    private readonly AppDbContext _context;
    private readonly IBackgroundJobExecutionMetricsService _metricsService;
    private readonly IBackgroundJobQueueProcessor _queueProcessor;

    public AdminJobService(
        AppDbContext context,
        IEnumerable<IBackgroundJobProcessor> jobs,
        IBackgroundJobExecutionMetricsService metricsService,
        IBackgroundJobQueueProcessor queueProcessor)
    {
        _context = context;
        _jobs = jobs;
        _metricsService = metricsService;
        _queueProcessor = queueProcessor;
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

    public async Task<IReadOnlyList<BackgroundJobFilaItemResponse>> ListarFilaAsync(CancellationToken cancellationToken = default)
    {
        var itens = await _context.BackgroundJobsExecucoes
            .AsNoTracking()
            .OrderByDescending(x => x.DataCriacao)
            .Take(100)
            .Select(x => new BackgroundJobFilaItemResponse
            {
                ExecucaoId = x.Id,
                JobId = x.JobId,
                NomeJob = x.NomeJob,
                Origem = x.Origem,
                SolicitadoPorAdminUsuarioId = x.SolicitadoPorAdminUsuarioId,
                Status = x.Status.ToString(),
                TentativasProcessamento = x.TentativasProcessamento,
                RegistrosProcessados = x.RegistrosProcessados,
                ProcessarAposUtc = x.ProcessarAposUtc,
                DataInicioProcessamento = x.DataInicioProcessamento,
                DataFinalizacao = x.DataFinalizacao,
                MensagemResultado = x.MensagemResultado,
                DataCriacao = x.DataCriacao
            })
            .ToListAsync(cancellationToken);

        return itens.AsReadOnly();
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

    public async Task<EnfileirarBackgroundJobAdminResponse?> EnfileirarAsync(string jobId, Guid? adminUsuarioId, CancellationToken cancellationToken = default)
    {
        var job = _jobs.FirstOrDefault(x => string.Equals(x.JobId, jobId, StringComparison.OrdinalIgnoreCase));
        if (job is null)
            return null;

        var execucao = new BackgroundJobExecucao
        {
            JobId = job.JobId,
            NomeJob = job.Nome,
            Origem = "manual-admin",
            SolicitadoPorAdminUsuarioId = adminUsuarioId,
            Status = StatusExecucaoBackgroundJob.Pendente,
            ProcessarAposUtc = DateTime.UtcNow,
            MensagemResultado = "Execução enfileirada."
        };

        _context.BackgroundJobsExecucoes.Add(execucao);
        await _context.SaveChangesAsync(cancellationToken);

        return new EnfileirarBackgroundJobAdminResponse
        {
            ExecucaoId = execucao.Id,
            JobId = execucao.JobId,
            NomeJob = execucao.NomeJob,
            Status = execucao.Status.ToString(),
            EnfileiradoEm = execucao.DataCriacao
        };
    }

    public async Task<ProcessarFilaBackgroundJobAdminResponse> ProcessarFilaAsync(CancellationToken cancellationToken = default)
    {
        var processadas = await _queueProcessor.ProcessarPendentesAsync(cancellationToken);
        return new ProcessarFilaBackgroundJobAdminResponse
        {
            ExecucoesProcessadas = processadas,
            ProcessadoEm = DateTime.UtcNow
        };
    }
}
