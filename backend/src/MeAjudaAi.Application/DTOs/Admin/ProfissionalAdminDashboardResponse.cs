namespace MeAjudaAi.Application.DTOs.Admin;

public class ProfissionalAdminDashboardResponse
{
    public ProfissionalAdminDetalheResponse Profissional { get; set; } = new();
    public UsuarioAdminDashboardNotificacoesResponse Notificacoes { get; set; } = new();
    public UsuarioAdminDashboardEmailsResponse Emails { get; set; } = new();
    public ProfissionalAdminDashboardServicosResponse Servicos { get; set; } = new();
    public ProfissionalAdminDashboardAvaliacoesResponse Avaliacoes { get; set; } = new();
    public ProfissionalAdminDashboardImpulsionamentosResponse Impulsionamentos { get; set; } = new();
}
