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

    public async Task<IReadOnlyList<BackgroundJobFilaItemResponse>> ListarFilaAsync(string? jobId = null, string? status = null, int? limit = null, CancellationToken cancellationToken = default)
    {
        var query = _context.BackgroundJobsExecucoes
            .AsNoTracking()
            .OrderByDescending(x => x.DataCriacao)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(jobId))
        {
            var jobIdNormalized = jobId.Trim().ToLowerInvariant();
            query = query.Where(x => x.JobId.ToLower() == jobIdNormalized);
        }

        var statusEnum = ParseStatus(status);
        if (statusEnum != null)
        {
            query = query.Where(x => x.Status == statusEnum.Value);
        }

        if (limit.HasValue && limit.Value > 0)
        {
            query = query.Take(limit.Value);
        }
        else
        {
            query = query.Take(100);
        }

        var itens = await query
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

    private static StatusExecucaoBackgroundJob? ParseStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return null;

        if (Enum.TryParse<StatusExecucaoBackgroundJob>(status, ignoreCase: true, out var resultado))
            return resultado;

        return null;
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

    public Task<EnfileirarBackgroundJobAdminResponse?> EnfileirarAsync(string jobId, Guid? adminUsuarioId, CancellationToken cancellationToken = default)
    {
        return CriarExecucaoAsync(jobId, adminUsuarioId, DateTime.UtcNow, "Execução enfileirada.", cancellationToken);
    }

    public Task<EnfileirarBackgroundJobAdminResponse?> AgendarAsync(string jobId, DateTime processarAposUtc, Guid? adminUsuarioId, CancellationToken cancellationToken = default)
    {
        return CriarExecucaoAsync(jobId, adminUsuarioId, processarAposUtc, "Execução agendada.", cancellationToken);
    }

    private async Task<EnfileirarBackgroundJobAdminResponse?> CriarExecucaoAsync(
        string jobId,
        Guid? adminUsuarioId,
        DateTime processarAposUtc,
        string mensagemResultado,
        CancellationToken cancellationToken)
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
            ProcessarAposUtc = processarAposUtc,
            MensagemResultado = mensagemResultado
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

    public async Task<BackgroundJobFilaItemResponse?> CancelarExecucaoAsync(Guid execucaoId, CancellationToken cancellationToken = default)
    {
        var execucao = await _context.BackgroundJobsExecucoes
            .FirstOrDefaultAsync(x => x.Id == execucaoId && x.Ativo, cancellationToken);

        if (execucao is null)
            return null;

        if (execucao.Status == StatusExecucaoBackgroundJob.Sucesso || execucao.Status == StatusExecucaoBackgroundJob.Cancelado)
            return null;

        execucao.Status = StatusExecucaoBackgroundJob.Cancelado;
        execucao.ProcessarAposUtc = null;
        execucao.DataAtualizacao = DateTime.UtcNow;
        execucao.MensagemResultado = "Execução cancelada manualmente.";

        await _context.SaveChangesAsync(cancellationToken);
        return MapearFila(execucao);
    }

    public async Task<BackgroundJobFilaItemResponse?> ReabrirExecucaoAsync(Guid execucaoId, CancellationToken cancellationToken = default)
    {
        var execucao = await _context.BackgroundJobsExecucoes
            .FirstOrDefaultAsync(x => x.Id == execucaoId && x.Ativo, cancellationToken);

        if (execucao is null)
            return null;

        if (execucao.Status == StatusExecucaoBackgroundJob.Processando || execucao.Status == StatusExecucaoBackgroundJob.Pendente)
            return null;

        execucao.Status = StatusExecucaoBackgroundJob.Pendente;
        execucao.ProcessarAposUtc = DateTime.UtcNow;
        execucao.DataInicioProcessamento = null;
        execucao.DataFinalizacao = null;
        execucao.MensagemResultado = "Execução reaberta manualmente.";
        execucao.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return MapearFila(execucao);
    }

    public async Task<BackgroundJobFilaMetricasResponse> ObterMetricasAsync(CancellationToken cancellationToken = default)
    {
        var execucoes = await _context.BackgroundJobsExecucoes
            .AsNoTracking()
            .Where(x => x.Ativo)
            .ToListAsync(cancellationToken);

        var agora = DateTime.UtcNow;

        static double AverageSeconds(IEnumerable<double> values)
        {
            return values.Any() ? values.Average() : 0d;
        }

        var temposFila = execucoes
            .Where(x => x.Status == StatusExecucaoBackgroundJob.Pendente)
            .Select(x => (agora - x.DataCriacao).TotalSeconds);

        var temposEspera = execucoes
            .Where(x => x.DataInicioProcessamento.HasValue)
            .Select(x => (x.DataInicioProcessamento!.Value - x.DataCriacao).TotalSeconds);

        var temposProcessamento = execucoes
            .Where(x => x.DataInicioProcessamento.HasValue && x.DataFinalizacao.HasValue)
            .Select(x => (x.DataFinalizacao!.Value - x.DataInicioProcessamento!.Value).TotalSeconds);

        var temposFalha = execucoes
            .Where(x =>
                x.Status == StatusExecucaoBackgroundJob.Falha &&
                x.DataInicioProcessamento.HasValue &&
                x.DataFinalizacao.HasValue)
            .Select(x => (x.DataFinalizacao!.Value - x.DataInicioProcessamento!.Value).TotalSeconds);

        var response = new BackgroundJobFilaMetricasResponse
        {
            TotalPendentes = execucoes.Count(x => x.Status == StatusExecucaoBackgroundJob.Pendente),
            TotalProcessando = execucoes.Count(x => x.Status == StatusExecucaoBackgroundJob.Processando),
            TotalSucesso = execucoes.Count(x => x.Status == StatusExecucaoBackgroundJob.Sucesso),
            TotalFalhas = execucoes.Count(x => x.Status == StatusExecucaoBackgroundJob.Falha),
            TotalCancelados = execucoes.Count(x => x.Status == StatusExecucaoBackgroundJob.Cancelado),
            PorJob = execucoes
                .GroupBy(x => x.JobId)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase)
            ,
            TempoMedioEsperaSegundos = AverageSeconds(temposEspera),
            TempoMedioProcessamentoSegundos = AverageSeconds(temposProcessamento),
            TempoMedioFalhaSegundos = AverageSeconds(temposFalha),
            TempoMedioFilaSegundos = AverageSeconds(temposFila)
        };

        return response;
    }

    public async Task<CancelarBackgroundJobAdminResponse> CancelarPorJobAsync(string jobId, CancellationToken cancellationToken = default)
    {
        var jobIdNormalized = jobId.Trim().ToLowerInvariant();
        var execucoes = await _context.BackgroundJobsExecucoes
            .Where(x =>
                x.Ativo &&
                x.JobId.ToLower() == jobIdNormalized &&
                x.Status != StatusExecucaoBackgroundJob.Sucesso &&
                x.Status != StatusExecucaoBackgroundJob.Cancelado)
            .ToListAsync(cancellationToken);

        if (execucoes.Count == 0)
        {
            return new CancelarBackgroundJobAdminResponse
            {
                JobId = jobId,
                Canceladas = 0
            };
        }

        var agora = DateTime.UtcNow;
        foreach (var execucao in execucoes)
        {
            execucao.Status = StatusExecucaoBackgroundJob.Cancelado;
            execucao.ProcessarAposUtc = null;
            execucao.MensagemResultado = "Execução cancelada manualmente em lote.";
            execucao.DataAtualizacao = agora;
            execucao.DataFinalizacao = agora;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new CancelarBackgroundJobAdminResponse
        {
            JobId = jobId,
            Canceladas = execucoes.Count
        };
    }

    private static BackgroundJobFilaItemResponse MapearFila(BackgroundJobExecucao x)
    {
        return new BackgroundJobFilaItemResponse
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
        };
    }
}
