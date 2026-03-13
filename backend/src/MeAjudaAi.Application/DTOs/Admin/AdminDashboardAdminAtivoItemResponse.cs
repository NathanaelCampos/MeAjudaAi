namespace MeAjudaAi.Application.DTOs.Admin;

public class AdminDashboardAdminAtivoItemResponse
{
    public Guid AdminUsuarioId { get; set; }
    public string NomeAdmin { get; set; } = string.Empty;
    public string EmailAdmin { get; set; } = string.Empty;
    public int TotalAcoes { get; set; }
    public DateTime? UltimaAcaoEm { get; set; }
}
