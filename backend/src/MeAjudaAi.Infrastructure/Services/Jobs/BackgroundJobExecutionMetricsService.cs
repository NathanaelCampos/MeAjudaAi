using System.Collections.Concurrent;
using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.Interfaces.Jobs;

namespace MeAjudaAi.Infrastructure.Services.Jobs;

public class BackgroundJobExecutionMetricsService : IBackgroundJobExecutionMetricsService
{
    private readonly ConcurrentDictionary<string, BackgroundJobAdminItemResponse> _jobs = new(StringComparer.OrdinalIgnoreCase);

    public BackgroundJobAdminItemResponse ObterSnapshot(string jobId, string nome, bool habilitado, int intervaloSegundos)
    {
        var snapshot = _jobs.GetOrAdd(jobId, _ => CriarSnapshot(jobId, nome, habilitado, intervaloSegundos));
        snapshot.JobId = jobId;
        snapshot.Nome = nome;
        snapshot.Habilitado = habilitado;
        snapshot.IntervaloSegundos = intervaloSegundos;
        return Copiar(snapshot);
    }

    public void RegistrarConfiguracao(string jobId, string nome, bool habilitado, int intervaloSegundos)
    {
        var snapshot = _jobs.GetOrAdd(jobId, _ => CriarSnapshot(jobId, nome, habilitado, intervaloSegundos));
        snapshot.JobId = jobId;
        snapshot.Nome = nome;
        snapshot.Habilitado = habilitado;
        snapshot.IntervaloSegundos = intervaloSegundos;
    }

    public void RegistrarInicio(string jobId, string nome, bool habilitado, int intervaloSegundos, DateTime iniciadoEm)
    {
        var snapshot = _jobs.GetOrAdd(jobId, _ => CriarSnapshot(jobId, nome, habilitado, intervaloSegundos));
        snapshot.JobId = jobId;
        snapshot.Nome = nome;
        snapshot.Habilitado = habilitado;
        snapshot.IntervaloSegundos = intervaloSegundos;
        snapshot.EmExecucao = true;
        snapshot.UltimoStatus = "executando";
        snapshot.UltimaExecucaoIniciadaEm = iniciadoEm;
        snapshot.TotalExecucoes++;
    }

    public void RegistrarSucesso(string jobId, string nome, bool habilitado, int intervaloSegundos, DateTime finalizadoEm, int registrosProcessados)
    {
        var snapshot = _jobs.GetOrAdd(jobId, _ => CriarSnapshot(jobId, nome, habilitado, intervaloSegundos));
        snapshot.JobId = jobId;
        snapshot.Nome = nome;
        snapshot.Habilitado = habilitado;
        snapshot.IntervaloSegundos = intervaloSegundos;
        snapshot.EmExecucao = false;
        snapshot.UltimoStatus = "sucesso";
        snapshot.UltimaExecucaoFinalizadaEm = finalizadoEm;
        snapshot.UltimosRegistrosProcessados = registrosProcessados;
        snapshot.TotalSucessos++;
        snapshot.UltimaMensagemErro = string.Empty;
    }

    public void RegistrarErro(string jobId, string nome, bool habilitado, int intervaloSegundos, DateTime finalizadoEm, string mensagemErro)
    {
        var snapshot = _jobs.GetOrAdd(jobId, _ => CriarSnapshot(jobId, nome, habilitado, intervaloSegundos));
        snapshot.JobId = jobId;
        snapshot.Nome = nome;
        snapshot.Habilitado = habilitado;
        snapshot.IntervaloSegundos = intervaloSegundos;
        snapshot.EmExecucao = false;
        snapshot.UltimoStatus = "falha";
        snapshot.UltimaExecucaoFinalizadaEm = finalizadoEm;
        snapshot.TotalFalhas++;
        snapshot.UltimaMensagemErro = mensagemErro;
    }

    private static BackgroundJobAdminItemResponse CriarSnapshot(string jobId, string nome, bool habilitado, int intervaloSegundos)
    {
        return new BackgroundJobAdminItemResponse
        {
            JobId = jobId,
            Nome = nome,
            Habilitado = habilitado,
            IntervaloSegundos = intervaloSegundos
        };
    }

    private static BackgroundJobAdminItemResponse Copiar(BackgroundJobAdminItemResponse snapshot)
    {
        return new BackgroundJobAdminItemResponse
        {
            JobId = snapshot.JobId,
            Nome = snapshot.Nome,
            Habilitado = snapshot.Habilitado,
            IntervaloSegundos = snapshot.IntervaloSegundos,
            EmExecucao = snapshot.EmExecucao,
            UltimoStatus = snapshot.UltimoStatus,
            UltimaExecucaoIniciadaEm = snapshot.UltimaExecucaoIniciadaEm,
            UltimaExecucaoFinalizadaEm = snapshot.UltimaExecucaoFinalizadaEm,
            UltimosRegistrosProcessados = snapshot.UltimosRegistrosProcessados,
            TotalExecucoes = snapshot.TotalExecucoes,
            TotalSucessos = snapshot.TotalSucessos,
            TotalFalhas = snapshot.TotalFalhas,
            UltimaMensagemErro = snapshot.UltimaMensagemErro
        };
    }
}
