namespace MeAjudaAi.Application.DTOs.Admin;

public class AdminDashboardWebhookFalhoItemResponse
{
    public Guid Id { get; set; }
    public string Provedor { get; set; } = string.Empty;
    public string EventoExternoId { get; set; } = string.Empty;
    public string CodigoReferenciaPagamento { get; set; } = string.Empty;
    public string MensagemResultado { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
}
