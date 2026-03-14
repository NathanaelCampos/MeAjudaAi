namespace MeAjudaAi.Application.DTOs.Admin;

public class BackgroundJobFilaMetricasResponse
{
    public int TotalPendentes { get; set; }
    public int TotalProcessando { get; set; }
    public int TotalSucesso { get; set; }
    public int TotalFalhas { get; set; }
    public int TotalCancelados { get; set; }
    public Dictionary<string, int> PorJob { get; set; } = new();
}
