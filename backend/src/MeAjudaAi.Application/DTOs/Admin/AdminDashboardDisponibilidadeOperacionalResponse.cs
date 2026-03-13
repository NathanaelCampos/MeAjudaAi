namespace MeAjudaAi.Application.DTOs.Admin;

public class AdminDashboardDisponibilidadeOperacionalResponse
{
    public decimal PercentualSucessoWebhooks { get; set; }
    public decimal PercentualFalhaWebhooks { get; set; }
    public decimal PercentualSucessoEmails { get; set; }
    public decimal PercentualFalhaEmails { get; set; }
}
