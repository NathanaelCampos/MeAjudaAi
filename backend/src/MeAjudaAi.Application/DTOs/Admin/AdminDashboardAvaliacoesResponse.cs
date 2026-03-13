namespace MeAjudaAi.Application.DTOs.Admin;

public class AdminDashboardAvaliacoesResponse
{
    public int Total { get; set; }
    public int Pendentes { get; set; }
    public int Aprovadas { get; set; }
    public int Rejeitadas { get; set; }
    public int Ocultas { get; set; }
}
