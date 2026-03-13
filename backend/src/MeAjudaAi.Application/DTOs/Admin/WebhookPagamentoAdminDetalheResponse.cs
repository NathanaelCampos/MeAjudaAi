namespace MeAjudaAi.Application.DTOs.Admin;

public class WebhookPagamentoAdminDetalheResponse
{
    public Guid Id { get; set; }
    public string Provedor { get; set; } = string.Empty;
    public string EventoExternoId { get; set; } = string.Empty;
    public string CodigoReferenciaPagamento { get; set; } = string.Empty;
    public string StatusPagamento { get; set; } = string.Empty;
    public bool ProcessadoComSucesso { get; set; }
    public string MensagemResultado { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public string HeadersJson { get; set; } = string.Empty;
    public string IpOrigem { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public Guid? ImpulsionamentoProfissionalId { get; set; }
    public int? StatusImpulsionamentoResultado { get; set; }
    public DateTime DataCriacao { get; set; }
}
