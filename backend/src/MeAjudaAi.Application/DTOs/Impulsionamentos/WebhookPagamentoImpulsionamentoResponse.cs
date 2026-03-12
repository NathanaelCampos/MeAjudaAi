namespace MeAjudaAi.Application.DTOs.Impulsionamentos;

public class WebhookPagamentoImpulsionamentoResponse
{
    public string Provedor { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public string EventoExternoId { get; set; } = string.Empty;
    public string StatusRecebido { get; set; } = string.Empty;
    public bool Duplicado { get; set; }
    public ImpulsionamentoProfissionalResponse? Impulsionamento { get; set; }
}
