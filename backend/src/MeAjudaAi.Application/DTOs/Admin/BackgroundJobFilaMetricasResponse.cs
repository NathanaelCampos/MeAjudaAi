namespace MeAjudaAi.Application.DTOs.Admin;

public class BackgroundJobFilaMetricasResponse
{
    public int TotalPendentes { get; set; }
    public int TotalProcessando { get; set; }
    public int TotalSucesso { get; set; }
    public int TotalFalhas { get; set; }
    public int TotalCancelados { get; set; }
    public Dictionary<string, int> PorJob { get; set; } = new();
    public double TempoMedioEsperaSegundos { get; set; }
    public double TempoMedioProcessamentoSegundos { get; set; }
    public double TempoMedioFalhaSegundos { get; set; }
    public double TempoMedioFilaSegundos { get; set; }
}
