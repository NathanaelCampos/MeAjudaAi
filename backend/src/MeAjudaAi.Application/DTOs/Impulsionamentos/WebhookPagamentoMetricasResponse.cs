namespace MeAjudaAi.Application.DTOs.Impulsionamentos;

public class WebhookPagamentoMetricasResponse
{
    public IReadOnlyList<WebhookPagamentoMetricaItemResponse> Itens { get; set; } = Array.Empty<WebhookPagamentoMetricaItemResponse>();
}
