namespace MeAjudaAi.Application.DTOs.Admin;

public class BackgroundJobFilaAlertaResponse
{
    public string JobId { get; set; } = string.Empty;
    public double TempoMedioFilaSegundos { get; set; }
    public double TempoMedioProcessamentoSegundos { get; set; }
    public int TotalPendentes { get; set; }
    public int TotalFalhas { get; set; }
    public string NivelAlerta { get; set; } = string.Empty;
}
