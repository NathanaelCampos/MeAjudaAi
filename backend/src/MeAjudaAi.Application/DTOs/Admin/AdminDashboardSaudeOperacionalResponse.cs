namespace MeAjudaAi.Application.DTOs.Admin;

public class AdminDashboardSaudeOperacionalResponse
{
    public string Status { get; set; } = "saudavel";
    public string IndicadorCor { get; set; } = "verde";
    public string PrioridadeVisual { get; set; } = "baixa";
    public string Resumo { get; set; } = string.Empty;
}
