namespace MeAjudaAi.Application.DTOs.Admin;

public class BackgroundJobFilaAlertasHistoricoResponse
{
    public string JobId { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public double TempoMedioFilaSegundos { get; set; }
    public double TempoMedioProcessamentoSegundos { get; set; }
    public int TotalAlertas { get; set; }
    public int TotalPendentes { get; set; }
    public int TotalFalhas { get; set; }
}
