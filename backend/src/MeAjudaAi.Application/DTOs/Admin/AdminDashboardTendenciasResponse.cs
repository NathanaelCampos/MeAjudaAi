namespace MeAjudaAi.Application.DTOs.Admin;

public class AdminDashboardTendenciasResponse
{
    public AdminDashboardTendenciaItemResponse Servicos { get; set; } = new();
    public AdminDashboardTendenciaItemResponse Avaliacoes { get; set; } = new();
    public AdminDashboardTendenciaItemResponse Webhooks { get; set; } = new();
    public AdminDashboardTendenciaItemResponse Emails { get; set; } = new();
}
