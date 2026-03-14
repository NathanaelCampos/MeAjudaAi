using MeAjudaAi.Application.DTOs.Admin;

namespace MeAjudaAi.Application.Interfaces.Admin;

public interface IAdminJobService
{
    Task<IReadOnlyList<BackgroundJobAdminItemResponse>> ListarAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BackgroundJobFilaItemResponse>> ListarFilaAsync(string? jobId = null, string? status = null, int? limit = null, CancellationToken cancellationToken = default);
    Task<ExecutarBackgroundJobAdminResponse?> ExecutarAsync(string jobId, CancellationToken cancellationToken = default);
    Task<EnfileirarBackgroundJobAdminResponse?> EnfileirarAsync(string jobId, Guid? adminUsuarioId, CancellationToken cancellationToken = default);
    Task<EnfileirarBackgroundJobAdminResponse?> AgendarAsync(string jobId, DateTime processarAposUtc, Guid? adminUsuarioId, CancellationToken cancellationToken = default);
    Task<ProcessarFilaBackgroundJobAdminResponse> ProcessarFilaAsync(CancellationToken cancellationToken = default);
    Task<BackgroundJobFilaItemResponse?> CancelarExecucaoAsync(Guid execucaoId, CancellationToken cancellationToken = default);
    Task<BackgroundJobFilaItemResponse?> ReabrirExecucaoAsync(Guid execucaoId, CancellationToken cancellationToken = default);
    Task<BackgroundJobFilaMetricasResponse> ObterMetricasAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BackgroundJobFilaAlertaResponse>> ObterAlertasFilaAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BackgroundJobFilaAlertasHistoricoResponse>> ObterHistoricoAlertasAsync(int dias = 7, CancellationToken cancellationToken = default);
    Task<CancelarBackgroundJobAdminResponse> CancelarPorJobAsync(string jobId, CancellationToken cancellationToken = default);
}
