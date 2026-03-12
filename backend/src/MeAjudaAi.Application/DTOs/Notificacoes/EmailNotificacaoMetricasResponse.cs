namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class EmailNotificacaoMetricasResponse
{
    public IReadOnlyList<EmailNotificacaoMetricaItemResponse> Itens { get; set; } = Array.Empty<EmailNotificacaoMetricaItemResponse>();
}
