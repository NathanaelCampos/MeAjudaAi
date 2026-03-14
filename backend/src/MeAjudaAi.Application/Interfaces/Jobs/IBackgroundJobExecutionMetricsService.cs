using MeAjudaAi.Application.DTOs.Admin;

namespace MeAjudaAi.Application.Interfaces.Jobs;

public interface IBackgroundJobExecutionMetricsService
{
    BackgroundJobAdminItemResponse ObterSnapshot(string jobId, string nome, bool habilitado, int intervaloSegundos);
    void RegistrarConfiguracao(string jobId, string nome, bool habilitado, int intervaloSegundos);
    void RegistrarInicio(string jobId, string nome, bool habilitado, int intervaloSegundos, DateTime iniciadoEm);
    void RegistrarSucesso(string jobId, string nome, bool habilitado, int intervaloSegundos, DateTime finalizadoEm, int registrosProcessados);
    void RegistrarErro(string jobId, string nome, bool habilitado, int intervaloSegundos, DateTime finalizadoEm, string mensagemErro);
}
