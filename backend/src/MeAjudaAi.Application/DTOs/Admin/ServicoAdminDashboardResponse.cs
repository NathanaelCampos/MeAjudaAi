namespace MeAjudaAi.Application.DTOs.Admin;

public class ServicoAdminDashboardResponse
{
    public ServicoAdminDetalheResponse Servico { get; set; } = new();
    public ServicoAdminDashboardNotificacoesResponse Notificacoes { get; set; } = new();
    public ServicoAdminDashboardEmailsResponse Emails { get; set; } = new();
    public ServicoAdminDashboardAvaliacaoResponse? Avaliacao { get; set; }
}
