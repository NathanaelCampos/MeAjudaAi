namespace MeAjudaAi.Application.DTOs.Admin;

public class WebhookPagamentoAdminDashboardResponse
{
    public WebhookPagamentoAdminDetalheResponse Webhook { get; set; } = new();
    public ImpulsionamentoAdminDetalheResponse? Impulsionamento { get; set; }
    public UsuarioAdminDashboardNotificacoesResponse Notificacoes { get; set; } = new();
    public UsuarioAdminDashboardEmailsResponse Emails { get; set; } = new();
    public ImpulsionamentoAdminDashboardWebhooksResponse WebhooksRelacionados { get; set; } = new();
}
