namespace MeAjudaAi.Application.DTOs.Impulsionamentos;

public class WebhookPagamentoImpulsionamentoEventoResponse
{
    public Guid Id { get; set; }
    public string Provedor { get; set; } = string.Empty;
    public string EventoExternoId { get; set; } = string.Empty;
    public string CodigoReferenciaPagamento { get; set; } = string.Empty;
    public string StatusPagamento { get; set; } = string.Empty;
    public bool ProcessadoComSucesso { get; set; }
    public string MensagemResultado { get; set; } = string.Empty;
    public string IpOrigem { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public Guid? ImpulsionamentoProfissionalId { get; set; }
    public int? StatusImpulsionamentoResultado { get; set; }
    public DateTime DataCriacao { get; set; }
}
