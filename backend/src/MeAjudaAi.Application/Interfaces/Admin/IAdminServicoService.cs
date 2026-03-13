using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Common;

namespace MeAjudaAi.Application.Interfaces.Admin;

public interface IAdminServicoService
{
    Task<PaginacaoResponse<ServicoAdminListItemResponse>> BuscarAsync(
        BuscarServicosAdminRequest request,
        CancellationToken cancellationToken = default);

    Task<ServicoAdminDetalheResponse?> ObterPorIdAsync(
        Guid servicoId,
        CancellationToken cancellationToken = default);
}
