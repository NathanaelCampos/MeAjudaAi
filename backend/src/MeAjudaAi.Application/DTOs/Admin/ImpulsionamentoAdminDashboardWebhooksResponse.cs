namespace MeAjudaAi.Application.DTOs.Admin;

public class ImpulsionamentoAdminDashboardWebhooksResponse
{
    public int Total { get; set; }
    public int Sucessos { get; set; }
    public int Falhas { get; set; }
    public DateTime? UltimaDataCriacao { get; set; }
    public IReadOnlyList<MeAjudaAi.Application.DTOs.Impulsionamentos.WebhookPagamentoImpulsionamentoEventoResponse> Recentes { get; set; } =
        Array.Empty<MeAjudaAi.Application.DTOs.Impulsionamentos.WebhookPagamentoImpulsionamentoEventoResponse>();
}
