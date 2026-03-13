namespace MeAjudaAi.Application.DTOs.Admin;

public class AdminDashboardImpulsionamentosResponse
{
    public int Total { get; set; }
    public int PendentesPagamento { get; set; }
    public int Ativos { get; set; }
    public int Encerrados { get; set; }
    public int Cancelados { get; set; }
    public int Expirados { get; set; }
}
