namespace MeAjudaAi.Application.DTOs.Admin;

public class AdminDashboardSaudeOperacionalResponse
{
    public string Status { get; set; } = "saudavel";
    public string IndicadorCor { get; set; } = "verde";
    public string PrioridadeVisual { get; set; } = "baixa";
    public int OrdemAtencao { get; set; } = 3;
    public string AcaoPrimariaSugerida { get; set; } = string.Empty;
    public string DestinoOperacionalPrimario { get; set; } = "dashboard";
    public string Resumo { get; set; } = string.Empty;
}
