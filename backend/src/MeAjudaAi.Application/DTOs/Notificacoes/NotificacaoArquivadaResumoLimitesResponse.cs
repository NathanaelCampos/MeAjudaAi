using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class NotificacaoArquivadaResumoLimitesResponse
{
    public Guid? UsuarioId { get; set; }
    public TipoNotificacao? TipoNotificacao { get; set; }
    public DateTime? DataCriacaoInicial { get; set; }
    public DateTime? DataCriacaoFinal { get; set; }
    public int TotalRegistros { get; set; }
    public int LimiteRecomendado { get; set; }
    public bool ModoSeguro { get; set; }
    public List<NotificacaoArquivadaResumoLimiteItemResponse> Limites { get; set; } = [];
}
