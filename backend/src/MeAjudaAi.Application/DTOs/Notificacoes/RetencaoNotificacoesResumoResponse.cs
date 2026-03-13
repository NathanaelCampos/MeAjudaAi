namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class RetencaoNotificacoesResumoResponse
{
    public bool Habilitada { get; set; }
    public int DiasRetencao { get; set; }
    public int LoteProcessamento { get; set; }
    public bool SomenteLidas { get; set; }
    public DateTime? UltimaExecucaoIniciadaEm { get; set; }
    public DateTime? UltimaExecucaoFinalizadaEm { get; set; }
    public int? UltimaQuantidadeArquivada { get; set; }
    public long TotalArquivado { get; set; }
    public string UltimoStatus { get; set; } = "nao_executado";
    public string? UltimaMensagemErro { get; set; }
}
