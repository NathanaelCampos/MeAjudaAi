namespace MeAjudaAi.Application.DTOs.Admin;

public class AdminDashboardConfiguracaoResponse
{
    public string PresetPeriodo { get; set; } = "custom";
    public int JanelaQualidadeDias { get; set; }
    public int JanelaAcaoAdminRecenteHoras { get; set; }
    public int JanelaSerieDias { get; set; }
}
