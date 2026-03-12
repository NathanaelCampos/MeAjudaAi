using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class EmailNotificacaoMetricasSerieResponse
{
    public int TotalRegistros { get; set; }
    public TipoNotificacao? TipoNotificacao { get; set; }
    public string? EmailDestino { get; set; }
    public DateTime? DataCriacaoInicial { get; set; }
    public DateTime? DataCriacaoFinal { get; set; }
    public IReadOnlyList<EmailNotificacaoMetricaSerieItemResponse> Itens { get; set; } = Array.Empty<EmailNotificacaoMetricaSerieItemResponse>();
}
