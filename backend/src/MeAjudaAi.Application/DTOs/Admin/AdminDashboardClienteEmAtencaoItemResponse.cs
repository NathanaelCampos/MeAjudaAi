namespace MeAjudaAi.Application.DTOs.Admin;

public class AdminDashboardClienteEmAtencaoItemResponse
{
    public Guid ClienteId { get; set; }
    public Guid UsuarioId { get; set; }
    public string NomeExibicao { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int ServicosEmAberto { get; set; }
    public int NotificacoesNaoLidas { get; set; }
    public int EmailsComFalha { get; set; }
    public int ScoreAtencao { get; set; }
}
