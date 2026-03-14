namespace MeAjudaAi.Application.DTOs.Admin;

public class BackgroundJobFilaItemResponse
{
    public Guid ExecucaoId { get; set; }
    public string JobId { get; set; } = string.Empty;
    public string NomeJob { get; set; } = string.Empty;
    public string Origem { get; set; } = string.Empty;
    public Guid? SolicitadoPorAdminUsuarioId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TentativasProcessamento { get; set; }
    public int RegistrosProcessados { get; set; }
    public DateTime? ProcessarAposUtc { get; set; }
    public DateTime? DataInicioProcessamento { get; set; }
    public DateTime? DataFinalizacao { get; set; }
    public string MensagemResultado { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
}
