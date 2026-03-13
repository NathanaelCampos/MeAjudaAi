namespace MeAjudaAi.Application.DTOs.Admin;

public class AdminDashboardTendenciaItemResponse
{
    public int Ultimos7Dias { get; set; }
    public int SeteDiasAnteriores { get; set; }
    public decimal VariacaoPercentual { get; set; }
}
