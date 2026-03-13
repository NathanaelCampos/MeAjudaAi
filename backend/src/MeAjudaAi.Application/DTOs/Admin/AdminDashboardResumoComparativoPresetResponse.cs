namespace MeAjudaAi.Application.DTOs.Admin;

public class AdminDashboardResumoComparativoPresetResponse
{
    public bool Disponivel { get; set; }
    public string EixoPrincipal { get; set; } = string.Empty;
    public decimal VariacaoPrincipalPercentual { get; set; }
    public string DirecaoPrincipal { get; set; } = "estavel";
    public string Resumo { get; set; } = string.Empty;
    public string Recomendacao { get; set; } = string.Empty;
}
