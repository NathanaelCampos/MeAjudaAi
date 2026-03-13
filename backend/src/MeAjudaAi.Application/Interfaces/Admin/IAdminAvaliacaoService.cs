using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Common;

namespace MeAjudaAi.Application.Interfaces.Admin;

public interface IAdminAvaliacaoService
{
    Task<PaginacaoResponse<AvaliacaoAdminListItemResponse>> BuscarAsync(
        BuscarAvaliacoesAdminRequest request,
        CancellationToken cancellationToken = default);

    Task<AvaliacaoAdminDetalheResponse?> ObterPorIdAsync(
        Guid avaliacaoId,
        CancellationToken cancellationToken = default);

    Task<AvaliacaoAdminDashboardResponse?> ObterDashboardAsync(
        Guid avaliacaoId,
        CancellationToken cancellationToken = default);
}
