namespace MeAjudaAi.Application.DTOs.Impulsionamentos;

public class WebhookPagamentoMetricaItemResponse
{
    public string Provedor { get; set; } = string.Empty;
    public string Resultado { get; set; } = string.Empty;
    public string StatusRecebido { get; set; } = string.Empty;
    public long Quantidade { get; set; }
}
