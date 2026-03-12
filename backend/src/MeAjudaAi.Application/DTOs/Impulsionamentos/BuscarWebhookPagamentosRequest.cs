namespace MeAjudaAi.Application.DTOs.Impulsionamentos;

public class BuscarWebhookPagamentosRequest
{
    public string? EventoExternoId { get; set; }
    public string? CodigoReferenciaPagamento { get; set; }
    public string? Provedor { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 20;
}
