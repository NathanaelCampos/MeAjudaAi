namespace MeAjudaAi.Application.DTOs.Admin;

public class ImpulsionamentoAdminDashboardResponse
{
    public ImpulsionamentoAdminDetalheResponse Impulsionamento { get; set; } = new();
    public UsuarioAdminDashboardNotificacoesResponse Notificacoes { get; set; } = new();
    public UsuarioAdminDashboardEmailsResponse Emails { get; set; } = new();
    public ImpulsionamentoAdminDashboardWebhooksResponse Webhooks { get; set; } = new();
}
