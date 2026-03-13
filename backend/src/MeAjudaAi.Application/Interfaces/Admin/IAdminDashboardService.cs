using MeAjudaAi.Application.DTOs.Admin;

namespace MeAjudaAi.Application.Interfaces.Admin;

public interface IAdminDashboardService
{
    Task<AdminDashboardResponse> ObterAsync(
        BuscarAdminDashboardRequest? request = null,
        CancellationToken cancellationToken = default);
}
