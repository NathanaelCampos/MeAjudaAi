namespace MeAjudaAi.Application.DTOs.Admin;

public class ServicoAdminDashboardNotificacoesResponse
{
    public int TotalAtivas { get; set; }
    public int NaoLidas { get; set; }
    public int Lidas { get; set; }
    public int Arquivadas { get; set; }
    public DateTime? UltimaDataCriacao { get; set; }
}
