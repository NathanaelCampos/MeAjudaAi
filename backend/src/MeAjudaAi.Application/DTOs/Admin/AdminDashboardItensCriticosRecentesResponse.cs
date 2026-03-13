namespace MeAjudaAi.Application.DTOs.Admin;

public class AdminDashboardItensCriticosRecentesResponse
{
    public List<AdminDashboardWebhookFalhoItemResponse> WebhooksFalhos { get; set; } = [];
    public List<AdminDashboardEmailFalhoItemResponse> EmailsFalhos { get; set; } = [];
    public List<AdminDashboardAvaliacaoPendenteItemResponse> AvaliacoesPendentes { get; set; } = [];
}
