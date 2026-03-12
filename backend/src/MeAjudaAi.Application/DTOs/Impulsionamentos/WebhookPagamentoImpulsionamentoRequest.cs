namespace MeAjudaAi.Application.DTOs.Impulsionamentos;

public class WebhookPagamentoImpulsionamentoRequest
{
    public string CodigoReferenciaPagamento { get; set; } = string.Empty;
    public string StatusPagamento { get; set; } = string.Empty;
    public string? EventoExternoId { get; set; }
}
