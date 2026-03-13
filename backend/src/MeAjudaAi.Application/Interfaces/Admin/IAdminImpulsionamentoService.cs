using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Common;

namespace MeAjudaAi.Application.Interfaces.Admin;

public interface IAdminImpulsionamentoService
{
    Task<PaginacaoResponse<ImpulsionamentoAdminListItemResponse>> BuscarAsync(
        BuscarImpulsionamentosAdminRequest request,
        CancellationToken cancellationToken = default);

    Task<ImpulsionamentoAdminDetalheResponse?> ObterPorIdAsync(
        Guid impulsionamentoId,
        CancellationToken cancellationToken = default);

    Task<ImpulsionamentoAdminDashboardResponse?> ObterDashboardAsync(
        Guid impulsionamentoId,
        CancellationToken cancellationToken = default);
}
