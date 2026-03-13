namespace MeAjudaAi.Application.DTOs.Admin;

public class UsuarioAdminDashboardResponse
{
    public UsuarioAdminDetalheResponse Usuario { get; set; } = new();
    public UsuarioAdminDashboardNotificacoesResponse Notificacoes { get; set; } = new();
    public UsuarioAdminDashboardEmailsResponse Emails { get; set; } = new();
}
