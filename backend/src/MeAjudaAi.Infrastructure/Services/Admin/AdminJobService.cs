using System;
using System.Collections.Generic;
using System.Linq;
using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.Interfaces.Admin;
using MeAjudaAi.Application.Interfaces.Jobs;
using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Infrastructure.Configurations;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Infrastructure.Services.Admin;

public class AdminJobService : IAdminJobService
{
    private readonly IEnumerable<IBackgroundJobProcessor> _jobs;
    private readonly AppDbContext _context;
    private readonly IBackgroundJobExecutionMetricsService _metricsService;
    private readonly IBackgroundJobQueueProcessor _queueProcessor;
    private readonly JobsAlertOptions _alertOptions;

    public AdminJobService(
        AppDbContext context,
        IEnumerable<IBackgroundJobProcessor> jobs,
        IBackgroundJobExecutionMetricsService metricsService,
        IBackgroundJobQueueProcessor queueProcessor,
        IOptions<JobsAlertOptions> alertOptions)
    {
        _context = context;
        _jobs = jobs;
        _metricsService = metricsService;
        _queueProcessor = queueProcessor;
        _alertOptions = alertOptions.Value;
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

    public async Task<IReadOnlyList<BackgroundJobFilaAlertaResponse>> ObterAlertasFilaAsync(CancellationToken cancellationToken = default)
    {
        if (!_alertOptions.Habilitado)
            return Array.Empty<BackgroundJobFilaAlertaResponse>();

        var agora = DateTime.UtcNow;
        var execucoes = await _context.BackgroundJobsExecucoes
            .AsNoTracking()
            .Where(x => x.Ativo)
            .ToListAsync(cancellationToken);

        var alerts = execucoes
            .GroupBy(x => x.JobId, StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var tempoFila = g
                    .Where(x => x.Status == StatusExecucaoBackgroundJob.Pendente)
                    .Select(x => (agora - x.DataCriacao).TotalSeconds);

                var tempoProcessamento = g
                    .Where(x => x.DataInicioProcessamento.HasValue && x.DataFinalizacao.HasValue)
                    .Select(x => (x.DataFinalizacao!.Value - x.DataInicioProcessamento!.Value).TotalSeconds);

                var totalPendentes = g.Count(x => x.Status == StatusExecucaoBackgroundJob.Pendente);
                var totalFalhas = g.Count(x => x.Status == StatusExecucaoBackgroundJob.Falha);

                return new
                {
                    JobId = g.Key,
                    TempoFilaMedio = tempoFila.Any() ? tempoFila.Average() : 0d,
                    TempoProcessamentoMedio = tempoProcessamento.Any() ? tempoProcessamento.Average() : 0d,
                    totalPendentes,
                    totalFalhas
                };
            })
            .Where(x => x.TempoFilaMedio >= _alertOptions.TempoEsperaLimiteSegundos ||
                        x.TempoProcessamentoMedio >= _alertOptions.TempoProcessamentoLimiteSegundos ||
                        x.totalFalhas > 0)
            .Select(x =>
            {
                var nivel = BuildNivelAlerta(x.TempoFilaMedio, x.TempoProcessamentoMedio, x.totalFalhas);
                var (mensagem, cor) = BuildMensagemCor(nivel);
                return new BackgroundJobFilaAlertaResponse
                {
                    JobId = x.JobId,
                    TempoMedioFilaSegundos = x.TempoFilaMedio,
                    TempoMedioProcessamentoSegundos = x.TempoProcessamentoMedio,
                    TotalPendentes = x.totalPendentes,
                    TotalFalhas = x.totalFalhas,
                    NivelAlerta = nivel,
                    Mensagem = mensagem,
                    Cor = cor
                };
            })
            .ToList();

        var resultado = alerts.AsReadOnly();
        await RegistrarHistoricoAsync(resultado, cancellationToken);
        return resultado;
    }

    public async Task<IReadOnlyList<BackgroundJobFilaAlertasHistoricoResponse>> ObterHistoricoAlertasAsync(int dias = 7, CancellationToken cancellationToken = default)
    {
        var intervaloDias = Math.Max(dias, 1);
        var limite = DateTime.UtcNow.Date.AddDays(-intervaloDias + 1);

        var historico = await _context.BackgroundJobFilaAlertasHistorico
            .AsNoTracking()
            .Where(x => x.DataCriacao >= limite)
            .ToListAsync(cancellationToken);

        var agrupado = historico
            .GroupBy(x => new { JobIdKey = x.JobId.ToLowerInvariant(), Data = x.DataCriacao.Date })
            .Select(g => new BackgroundJobFilaAlertasHistoricoResponse
            {
                JobId = g.Select(x => x.JobId).FirstOrDefault() ?? g.Key.JobIdKey,
                Data = g.Key.Data,
                TempoMedioFilaSegundos = g.Average(x => x.TempoMedioFilaSegundos),
                TempoMedioProcessamentoSegundos = g.Average(x => x.TempoMedioProcessamentoSegundos),
                TotalAlertas = g.Count(),
                TotalPendentes = g.Sum(x => x.TotalPendentes),
                TotalFalhas = g.Sum(x => x.TotalFalhas)
            })
            .OrderByDescending(x => x.Data)
            .ThenBy(x => x.JobId)
            .ToList();

        return agrupado.AsReadOnly();
    }

    private async Task RegistrarHistoricoAsync(IEnumerable<BackgroundJobFilaAlertaResponse> alerts, CancellationToken cancellationToken)
    {
        if (!alerts.Any())
            return;

        var historico = alerts.Select(x => new BackgroundJobFilaAlertaHistorico
        {
            JobId = x.JobId,
            NivelAlerta = x.NivelAlerta,
            Mensagem = x.Mensagem,
            Cor = x.Cor,
            TempoMedioFilaSegundos = x.TempoMedioFilaSegundos,
            TempoMedioProcessamentoSegundos = x.TempoMedioProcessamentoSegundos,
            TotalPendentes = x.TotalPendentes,
            TotalFalhas = x.TotalFalhas
        });

        await _context.BackgroundJobFilaAlertasHistorico.AddRangeAsync(historico, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static string BuildNivelAlerta(double tempoFila, double tempoProcessamento, int totalFalhas)
    {
        if (totalFalhas > 0)
            return "Falhas";
        if (tempoFila > tempoProcessamento)
            return "Fila longa";
        return "Processamento lento";
    }

    private static (string Mensagem, string Cor) BuildMensagemCor(string nivel)
    {
        return nivel switch
        {
            "Falhas" => ("Há execuções com falhas recentes; verifique logs e retries.", "#D32F2F"),
            "Fila longa" => ("Fila crescendo; talvez aumentar workers ou ajustar lote.", "#F57C00"),
            "Processamento lento" => ("Jobs com tempo de processamento alto; revisar desempenho.", "#1976D2"),
            _ => ("Nenhum alerta crítico.", "#4CAF50")
        };
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
