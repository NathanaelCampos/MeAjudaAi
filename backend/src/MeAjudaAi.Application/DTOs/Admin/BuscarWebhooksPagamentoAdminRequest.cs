namespace MeAjudaAi.Application.DTOs.Admin;

public class BuscarWebhooksPagamentoAdminRequest
{
    public string? EventoExternoId { get; set; }
    public string? CodigoReferenciaPagamento { get; set; }
    public string? Provedor { get; set; }
    public bool? ProcessadoComSucesso { get; set; }
    public Guid? ImpulsionamentoProfissionalId { get; set; }
    public DateTime? DataCriacaoInicial { get; set; }
    public DateTime? DataCriacaoFinal { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 20;
}
