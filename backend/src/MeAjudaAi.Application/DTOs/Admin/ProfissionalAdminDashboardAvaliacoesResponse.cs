namespace MeAjudaAi.Application.DTOs.Admin;

public class ProfissionalAdminDashboardAvaliacoesResponse
{
    public int Total { get; set; }
    public int Pendentes { get; set; }
    public int Aprovadas { get; set; }
    public int Rejeitadas { get; set; }
    public int Ocultas { get; set; }
    public decimal? NotaMediaAtendimento { get; set; }
    public decimal? NotaMediaServico { get; set; }
    public decimal? NotaMediaPreco { get; set; }
    public DateTime? UltimaDataCriacao { get; set; }
}
