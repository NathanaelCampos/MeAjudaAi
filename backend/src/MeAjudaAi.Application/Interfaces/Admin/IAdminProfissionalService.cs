using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Common;

namespace MeAjudaAi.Application.Interfaces.Admin;

public interface IAdminProfissionalService
{
    Task<PaginacaoResponse<ProfissionalAdminListItemResponse>> BuscarAsync(
        BuscarProfissionaisAdminRequest request,
        CancellationToken cancellationToken = default);

    Task<ProfissionalAdminDetalheResponse?> ObterPorIdAsync(
        Guid profissionalId,
        CancellationToken cancellationToken = default);

    Task<ProfissionalAdminDashboardResponse?> ObterDashboardAsync(
        Guid profissionalId,
        CancellationToken cancellationToken = default);

    Task<ProfissionalAdminDetalheResponse> DefinirPerfilVerificadoAsync(
        Guid profissionalId,
        bool perfilVerificado,
        CancellationToken cancellationToken = default);

    Task<ProfissionalAdminDetalheResponse> DefinirAtivoAsync(
        Guid profissionalId,
        bool ativo,
        CancellationToken cancellationToken = default);
}
