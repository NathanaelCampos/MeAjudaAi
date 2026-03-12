using System.Text.Json;
using MeAjudaAi.Application.DTOs.Impulsionamentos;

namespace MeAjudaAi.Api.Webhooks;

public class AsaasWebhookPagamentoPayloadAdapter : IWebhookPagamentoPayloadAdapter
{
    public string Provedor => "asaas";

    public WebhookPagamentoImpulsionamentoRequest? Parse(string corpoBruto)
    {
        var payload = JsonSerializer.Deserialize<AsaasWebhookPagamentoRequest>(
            corpoBruto,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        if (payload is null)
            return null;

        var codigoReferencia = payload.Payment?.ExternalReference?.Trim() ?? string.Empty;
        var eventoExternoId = payload.Id?.Trim();
        var statusNormalizado = NormalizarStatus(payload);

        return new WebhookPagamentoImpulsionamentoRequest
        {
            CodigoReferenciaPagamento = codigoReferencia,
            EventoExternoId = eventoExternoId,
            StatusPagamento = statusNormalizado
        };
    }

    private static string NormalizarStatus(AsaasWebhookPagamentoRequest payload)
    {
        var evento = payload.Event?.Trim().ToUpperInvariant() ?? string.Empty;
        var statusPagamento = payload.Payment?.Status?.Trim().ToUpperInvariant() ?? string.Empty;

        return evento switch
        {
            "PAYMENT_RECEIVED" or "PAYMENT_CONFIRMED" => "pago",
            "PAYMENT_DELETED" => "cancelado",
            "PAYMENT_REFUNDED" or "PAYMENT_CHARGEBACK_REQUESTED" or "PAYMENT_CHARGEBACK_DISPUTE" => "estornado",
            "PAYMENT_OVERDUE" => "expirado",
            _ => statusPagamento switch
            {
                "RECEIVED" or "CONFIRMED" => "pago",
                "DELETED" => "cancelado",
                "REFUNDED" or "CHARGEBACK_REQUESTED" or "CHARGEBACK_DISPUTE" => "estornado",
                "OVERDUE" => "expirado",
                _ => string.Empty
            }
        };
    }

    private sealed class AsaasWebhookPagamentoRequest
    {
        public string? Id { get; set; }
        public string? Event { get; set; }
        public AsaasWebhookPagamentoInfo? Payment { get; set; }
    }

    private sealed class AsaasWebhookPagamentoInfo
    {
        public string? ExternalReference { get; set; }
        public string? Status { get; set; }
    }
}
