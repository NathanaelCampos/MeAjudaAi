using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Common;

namespace MeAjudaAi.Application.Interfaces.Admin;

public interface IAdminUsuarioService
{
    Task<PaginacaoResponse<UsuarioAdminListItemResponse>> BuscarAsync(
        BuscarUsuariosAdminRequest request,
        CancellationToken cancellationToken = default);

    Task<UsuarioAdminDetalheResponse?> ObterPorIdAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    Task<UsuarioAdminDashboardResponse?> ObterDashboardAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    Task<UsuarioAdminDetalheResponse> DefinirAtivoAsync(
        Guid usuarioId,
        bool ativo,
        Guid? usuarioAdministradorId = null,
        CancellationToken cancellationToken = default);
}
