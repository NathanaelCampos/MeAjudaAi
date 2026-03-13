namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class PreviewArquivamentoNotificacoesResponse
{
    public int QuantidadeCandidata { get; set; }
    public IReadOnlyList<NotificacaoAdminResponse> Recentes { get; set; } = Array.Empty<NotificacaoAdminResponse>();
}
