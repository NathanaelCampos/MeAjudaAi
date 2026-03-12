namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class EmailNotificacaoDestinatarioMetricaItemResponse
{
    public Guid UsuarioId { get; set; }
    public string EmailDestino { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Pendentes { get; set; }
    public int Enviados { get; set; }
    public int Falhas { get; set; }
    public int Cancelados { get; set; }
}
