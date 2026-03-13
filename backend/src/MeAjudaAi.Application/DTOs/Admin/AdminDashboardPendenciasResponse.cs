namespace MeAjudaAi.Application.DTOs.Admin;

public class AdminDashboardPendenciasResponse
{
    public int AvaliacoesPendentesModeracao { get; set; }
    public int ImpulsionamentosPendentesPagamento { get; set; }
    public int ServicosSolicitados { get; set; }
    public int NotificacoesNaoLidas { get; set; }
    public int EmailsPendentes { get; set; }
}
