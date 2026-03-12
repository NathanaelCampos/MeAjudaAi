using MeAjudaAi.Application.DTOs.Impulsionamentos;
using MeAjudaAi.Application.DTOs.Common;

namespace MeAjudaAi.Application.Interfaces.Impulsionamentos;

public interface IImpulsionamentoService
{
    Task<IReadOnlyList<PlanoImpulsionamentoResponse>> ListarPlanosAsync(
        CancellationToken cancellationToken = default);

    Task<ImpulsionamentoProfissionalResponse> ContratarPlanoAsync(
        Guid usuarioId,
        ContratarPlanoImpulsionamentoRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ImpulsionamentoProfissionalResponse>> ListarMeusImpulsionamentosAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    Task<PaginacaoResponse<WebhookPagamentoImpulsionamentoEventoResponse>> ListarWebhooksAsync(
        BuscarWebhookPagamentosRequest request,
        CancellationToken cancellationToken = default);

    Task<ImpulsionamentoProfissionalResponse> ConfirmarPagamentoAsync(
        Guid impulsionamentoId,
        CancellationToken cancellationToken = default);

    Task<ImpulsionamentoProfissionalResponse> ConfirmarPagamentoPorCodigoReferenciaAsync(
        string codigoReferenciaPagamento,
        CancellationToken cancellationToken = default);

    Task<WebhookPagamentoImpulsionamentoResponse> ProcessarWebhookPagamentoAsync(
        string provedor,
        WebhookPagamentoImpulsionamentoRequest request,
        string payloadJson,
        string headersJson,
        string ipOrigem,
        string requestId,
        string userAgent,
        CancellationToken cancellationToken = default);

    Task<ImpulsionamentoProfissionalResponse> CancelarPorCodigoReferenciaAsync(
        string codigoReferenciaPagamento,
        CancellationToken cancellationToken = default);
}
