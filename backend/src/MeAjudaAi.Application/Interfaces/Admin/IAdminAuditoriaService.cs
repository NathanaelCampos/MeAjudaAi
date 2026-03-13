using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Common;

namespace MeAjudaAi.Application.Interfaces.Admin;

public interface IAdminAuditoriaService
{
    Task RegistrarAsync(
        Guid adminUsuarioId,
        string entidade,
        Guid entidadeId,
        string acao,
        string descricao,
        string? payloadJson = null,
        CancellationToken cancellationToken = default);

    Task<PaginacaoResponse<AuditoriaAdminListItemResponse>> BuscarAsync(
        BuscarAuditoriasAdminRequest request,
        CancellationToken cancellationToken = default);

    Task<AuditoriaAdminDetalheResponse?> ObterPorIdAsync(
        Guid auditoriaId,
        CancellationToken cancellationToken = default);
}
