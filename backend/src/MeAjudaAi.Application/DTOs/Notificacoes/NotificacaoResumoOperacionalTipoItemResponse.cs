using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class NotificacaoResumoOperacionalTipoItemResponse
{
    public TipoNotificacao TipoNotificacao { get; set; }
    public int Total { get; set; }
    public int Lidas { get; set; }
    public int NaoLidas { get; set; }
}
