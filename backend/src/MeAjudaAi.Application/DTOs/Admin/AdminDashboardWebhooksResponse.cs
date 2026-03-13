namespace MeAjudaAi.Application.DTOs.Admin;

public class AdminDashboardWebhooksResponse
{
    public int Total { get; set; }
    public int Sucessos { get; set; }
    public int Falhas { get; set; }
    public DateTime? UltimaDataCriacao { get; set; }
}
