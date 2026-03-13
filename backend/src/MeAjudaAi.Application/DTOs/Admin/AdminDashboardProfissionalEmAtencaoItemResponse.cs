namespace MeAjudaAi.Application.DTOs.Admin;

public class AdminDashboardProfissionalEmAtencaoItemResponse
{
    public Guid ProfissionalId { get; set; }
    public Guid UsuarioId { get; set; }
    public string NomeExibicao { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int ServicosSolicitados { get; set; }
    public int AvaliacoesPendentes { get; set; }
    public int ImpulsionamentosPendentesPagamento { get; set; }
    public int WebhooksFalhos { get; set; }
    public int EmailsComFalha { get; set; }
    public int ScoreAtencao { get; set; }
}
