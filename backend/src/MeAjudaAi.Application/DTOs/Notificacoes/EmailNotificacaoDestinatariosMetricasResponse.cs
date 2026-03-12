using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class EmailNotificacaoDestinatariosMetricasResponse
{
    public int TotalRegistros { get; set; }
    public int TotalDestinatarios { get; set; }
    public TipoNotificacao? TipoNotificacao { get; set; }
    public string? EmailDestino { get; set; }
    public DateTime? DataCriacaoInicial { get; set; }
    public DateTime? DataCriacaoFinal { get; set; }
    public IReadOnlyList<EmailNotificacaoDestinatarioMetricaItemResponse> Itens { get; set; } = Array.Empty<EmailNotificacaoDestinatarioMetricaItemResponse>();
}
