using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.DTOs.Impulsionamentos;

namespace MeAjudaAi.Application.Interfaces.Admin;

public interface IAdminWebhookPagamentoService
{
    Task<PaginacaoResponse<WebhookPagamentoImpulsionamentoEventoResponse>> BuscarAsync(
        BuscarWebhooksPagamentoAdminRequest request,
        CancellationToken cancellationToken = default);

    Task<WebhookPagamentoAdminDetalheResponse?> ObterPorIdAsync(
        Guid webhookId,
        CancellationToken cancellationToken = default);
}
