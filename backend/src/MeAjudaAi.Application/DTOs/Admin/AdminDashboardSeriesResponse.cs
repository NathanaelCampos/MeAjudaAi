namespace MeAjudaAi.Application.DTOs.Admin;

public class AdminDashboardSeriesResponse
{
    public IReadOnlyList<AdminDashboardSerieItemResponse> Servicos { get; set; } = Array.Empty<AdminDashboardSerieItemResponse>();
    public IReadOnlyList<AdminDashboardSerieItemResponse> Avaliacoes { get; set; } = Array.Empty<AdminDashboardSerieItemResponse>();
    public IReadOnlyList<AdminDashboardSerieItemResponse> Webhooks { get; set; } = Array.Empty<AdminDashboardSerieItemResponse>();
    public IReadOnlyList<AdminDashboardSerieItemResponse> Emails { get; set; } = Array.Empty<AdminDashboardSerieItemResponse>();
}
