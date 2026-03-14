using MeAjudaAi.Application.DTOs.Admin;

namespace MeAjudaAi.Application.Interfaces.Admin;

public interface IAdminJobService
{
    Task<IReadOnlyList<BackgroundJobAdminItemResponse>> ListarAsync(CancellationToken cancellationToken = default);
    Task<ExecutarBackgroundJobAdminResponse?> ExecutarAsync(string jobId, CancellationToken cancellationToken = default);
}
