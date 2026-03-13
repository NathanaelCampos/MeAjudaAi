namespace MeAjudaAi.Application.DTOs.Admin;

public class BuscarAdminDashboardRequest
{
    public string? PresetPeriodo { get; set; }
    public int? JanelaQualidadeDias { get; set; }
    public int? JanelaAcaoAdminRecenteHoras { get; set; }
    public int? JanelaSerieDias { get; set; }
}
