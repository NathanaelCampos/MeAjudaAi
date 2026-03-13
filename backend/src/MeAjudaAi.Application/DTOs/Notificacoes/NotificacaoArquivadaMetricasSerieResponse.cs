using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class NotificacaoArquivadaMetricasSerieResponse
{
    public Guid? UsuarioId { get; set; }
    public TipoNotificacao? TipoNotificacao { get; set; }
    public DateTime? DataCriacaoInicial { get; set; }
    public DateTime? DataCriacaoFinal { get; set; }
    public int TotalRegistros { get; set; }
    public IReadOnlyList<NotificacaoArquivadaMetricaSerieItemResponse> Itens { get; set; } =
        Array.Empty<NotificacaoArquivadaMetricaSerieItemResponse>();
}
