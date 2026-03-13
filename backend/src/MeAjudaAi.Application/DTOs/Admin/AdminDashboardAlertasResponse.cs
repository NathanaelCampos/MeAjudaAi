namespace MeAjudaAi.Application.DTOs.Admin;

public class AdminDashboardAlertasResponse
{
    public int WebhooksFalhos { get; set; }
    public int EmailsComFalha { get; set; }
    public int EmailsPendentesAtrasados { get; set; }
    public bool SemAcaoAdminRecenteSobRisco { get; set; }
    public DateTime? UltimaAcaoAdminEm { get; set; }
}
