namespace MeAjudaAi.Application.DTOs.Admin;

public class AvaliacaoAdminDashboardResponse
{
    public AvaliacaoAdminDetalheResponse Avaliacao { get; set; } = new();
    public AvaliacaoAdminDashboardServicoResponse Servico { get; set; } = new();
    public AvaliacaoAdminDashboardNotificacoesResponse Notificacoes { get; set; } = new();
    public AvaliacaoAdminDashboardEmailsResponse Emails { get; set; } = new();
}
