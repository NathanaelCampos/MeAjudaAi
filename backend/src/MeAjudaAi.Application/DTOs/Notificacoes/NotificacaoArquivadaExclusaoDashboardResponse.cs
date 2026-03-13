using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class NotificacaoArquivadaExclusaoDashboardResponse
{
    public Guid? UsuarioId { get; set; }
    public TipoNotificacao? TipoNotificacao { get; set; }
    public DateTime? DataCriacaoInicial { get; set; }
    public DateTime? DataCriacaoFinal { get; set; }
    public NotificacaoResumoOperacionalResponse Resumo { get; set; } = new();
    public NotificacaoArquivadaResumoLeituraResponse Leitura { get; set; } = new();
    public NotificacaoArquivadaMetricasSerieResponse Serie { get; set; } = new();
    public NotificacaoArquivadaResumoIdadeResponse Idade { get; set; } = new();
    public NotificacaoArquivadaResumoTiposResponse Tipos { get; set; } = new();
    public NotificacaoArquivadaResumoUsuariosResponse Usuarios { get; set; } = new();
    public NotificacaoArquivadaResumoLimitesResponse Limites { get; set; } = new();
    public PreviewExclusaoNotificacoesAntigasResponse Antigas { get; set; } = new();
}
