using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class NotificacaoUsuarioDashboardResponse
{
    public Guid UsuarioId { get; set; }
    public TipoNotificacao? TipoNotificacao { get; set; }
    public DateTime? DataCriacaoInicial { get; set; }
    public DateTime? DataCriacaoFinal { get; set; }
    public NotificacaoResumoOperacionalResponse Resumo { get; set; } = new();
    public IReadOnlyList<NotificacaoAdminResponse> Recentes { get; set; } = Array.Empty<NotificacaoAdminResponse>();
}
