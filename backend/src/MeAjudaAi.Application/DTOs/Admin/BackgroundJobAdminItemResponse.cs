namespace MeAjudaAi.Application.DTOs.Admin;

public class BackgroundJobAdminItemResponse
{
    public string JobId { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public bool Habilitado { get; set; }
    public int IntervaloSegundos { get; set; }
    public bool EmExecucao { get; set; }
    public string UltimoStatus { get; set; } = "nunca_executado";
    public DateTime? UltimaExecucaoIniciadaEm { get; set; }
    public DateTime? UltimaExecucaoFinalizadaEm { get; set; }
    public int? UltimosRegistrosProcessados { get; set; }
    public int TotalExecucoes { get; set; }
    public int TotalSucessos { get; set; }
    public int TotalFalhas { get; set; }
    public string UltimaMensagemErro { get; set; } = string.Empty;
}
