using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class NotificacaoArquivadaResumoUsuariosResponse
{
    public Guid? UsuarioId { get; set; }
    public TipoNotificacao? TipoNotificacao { get; set; }
    public DateTime? DataCriacaoInicial { get; set; }
    public DateTime? DataCriacaoFinal { get; set; }
    public int TotalRegistros { get; set; }
    public IReadOnlyList<NotificacaoResumoOperacionalUsuarioItemResponse> Usuarios { get; set; } =
        Array.Empty<NotificacaoResumoOperacionalUsuarioItemResponse>();
}
