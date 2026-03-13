namespace MeAjudaAi.Application.DTOs.Admin;

public class AdminDashboardComparativoPresetItemResponse
{
    public int TotalPresetAtual { get; set; }
    public int TotalPresetAnterior { get; set; }
    public decimal VariacaoPercentual { get; set; }
}
