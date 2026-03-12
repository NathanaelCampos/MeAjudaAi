namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class EmailNotificacaoTiposMetricasResponse
{
    public int TotalRegistros { get; set; }
    public string? EmailDestino { get; set; }
    public DateTime? DataCriacaoInicial { get; set; }
    public DateTime? DataCriacaoFinal { get; set; }
    public IReadOnlyList<EmailNotificacaoTipoMetricaItemResponse> Itens { get; set; } = Array.Empty<EmailNotificacaoTipoMetricaItemResponse>();
}
