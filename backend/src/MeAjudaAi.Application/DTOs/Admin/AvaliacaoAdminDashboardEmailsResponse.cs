namespace MeAjudaAi.Application.DTOs.Admin;

public class AvaliacaoAdminDashboardEmailsResponse
{
    public int Total { get; set; }
    public int Pendentes { get; set; }
    public int Enviados { get; set; }
    public int Falhas { get; set; }
    public int Cancelados { get; set; }
    public DateTime? UltimaDataCriacao { get; set; }
}
