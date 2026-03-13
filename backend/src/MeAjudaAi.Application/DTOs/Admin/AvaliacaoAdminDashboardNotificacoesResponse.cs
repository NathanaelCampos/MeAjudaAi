namespace MeAjudaAi.Application.DTOs.Admin;

public class AvaliacaoAdminDashboardNotificacoesResponse
{
    public int Total { get; set; }
    public int Lidas { get; set; }
    public int NaoLidas { get; set; }
    public int Arquivadas { get; set; }
    public DateTime? UltimaDataCriacao { get; set; }
}
