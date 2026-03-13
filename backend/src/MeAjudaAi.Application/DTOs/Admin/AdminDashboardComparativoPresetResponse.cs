namespace MeAjudaAi.Application.DTOs.Admin;

public class AdminDashboardComparativoPresetResponse
{
    public bool Disponivel { get; set; }
    public string PresetAtual { get; set; } = "custom";
    public string? PresetAnterior { get; set; }
    public int JanelaAtualDias { get; set; }
    public int JanelaAnteriorDias { get; set; }
    public AdminDashboardComparativoPresetItemResponse Servicos { get; set; } = new();
    public AdminDashboardComparativoPresetItemResponse Avaliacoes { get; set; } = new();
    public AdminDashboardComparativoPresetItemResponse Webhooks { get; set; } = new();
    public AdminDashboardComparativoPresetItemResponse Emails { get; set; } = new();
}
