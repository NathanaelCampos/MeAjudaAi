namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class PreviewExclusaoNotificacoesAntigasResponse
{
    public int QuantidadeCandidata { get; set; }
    public IReadOnlyList<NotificacaoAdminResponse> Antigas { get; set; } = Array.Empty<NotificacaoAdminResponse>();
}
