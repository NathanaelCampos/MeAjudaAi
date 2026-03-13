namespace MeAjudaAi.Application.DTOs.Admin;

public class AdminDashboardServicosResponse
{
    public int Total { get; set; }
    public int Solicitados { get; set; }
    public int Aceitos { get; set; }
    public int EmExecucao { get; set; }
    public int Concluidos { get; set; }
    public int Cancelados { get; set; }
}
