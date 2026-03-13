namespace MeAjudaAi.Application.DTOs.Admin;

public class AdminDashboardSlaOperacionalResponse
{
    public DateTime? UltimaAcaoAdminEm { get; set; }
    public int? MinutosDesdeUltimaAcaoAdmin { get; set; }
    public DateTime? UltimoWebhookFalhoEm { get; set; }
    public int? MinutosDesdeUltimoWebhookFalho { get; set; }
    public DateTime? UltimoEmailProcessadoEm { get; set; }
    public int? MinutosDesdeUltimoEmailProcessado { get; set; }
}
