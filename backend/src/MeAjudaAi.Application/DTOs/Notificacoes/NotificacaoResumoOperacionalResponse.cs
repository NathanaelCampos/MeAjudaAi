using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class NotificacaoResumoOperacionalResponse
{
    public Guid? UsuarioId { get; set; }
    public TipoNotificacao? TipoNotificacao { get; set; }
    public DateTime? DataCriacaoInicial { get; set; }
    public DateTime? DataCriacaoFinal { get; set; }
    public int TotalRegistros { get; set; }
    public int Lidas { get; set; }
    public int NaoLidas { get; set; }
    public IReadOnlyList<NotificacaoResumoOperacionalTipoItemResponse> TopTipos { get; set; } =
        Array.Empty<NotificacaoResumoOperacionalTipoItemResponse>();
    public IReadOnlyList<NotificacaoResumoOperacionalUsuarioItemResponse> TopUsuariosComNaoLidas { get; set; } =
        Array.Empty<NotificacaoResumoOperacionalUsuarioItemResponse>();
}
