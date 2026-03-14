namespace MeAjudaAi.Application.DTOs.Admin;

public class AdminDashboardResponse
{
    public AdminDashboardConfiguracaoResponse Configuracao { get; set; } = new();
    public AdminDashboardUsuariosResponse Usuarios { get; set; } = new();
    public AdminDashboardProfissionaisResponse Profissionais { get; set; } = new();
    public AdminDashboardServicosResponse Servicos { get; set; } = new();
    public AdminDashboardAvaliacoesResponse Avaliacoes { get; set; } = new();
    public AdminDashboardImpulsionamentosResponse Impulsionamentos { get; set; } = new();
    public AdminDashboardWebhooksResponse Webhooks { get; set; } = new();
    public AdminDashboardNotificacoesResponse Notificacoes { get; set; } = new();
    public AdminDashboardEmailsResponse Emails { get; set; } = new();
    public AdminDashboardSeriesResponse Series { get; set; } = new();
    public AdminDashboardTendenciasResponse Tendencias { get; set; } = new();
    public AdminDashboardComparativoPresetResponse ComparativoPresetAnterior { get; set; } = new();
    public AdminDashboardInsightComparativoPresetResponse InsightComparativoPrincipal { get; set; } = new();
    public string EixoComparativoPrincipal { get; set; } = string.Empty;
    public decimal? VariacaoComparativaPrincipal { get; set; }
    public string DirecaoComparativaPrincipal { get; set; } = "indisponivel";
    public string StatusComparativoPrincipal { get; set; } = "indisponivel";
    public string IndicadorComparativoPrincipal { get; set; } = "cinza";
    public string PrioridadeComparativaPrincipal { get; set; } = "baixa";
    public string AcaoComparativaPrincipal { get; set; } = string.Empty;
    public string LinkComparativoPrincipal { get; set; } = "/admin/dashboard";
    public string TooltipComparativoPrincipal { get; set; } = string.Empty;
    public AdminDashboardPendenciasResponse Pendencias { get; set; } = new();
    public AdminDashboardAlertasResponse Alertas { get; set; } = new();
    public string RiscoOperacional { get; set; } = "baixo";
    public AdminDashboardItensCriticosRecentesResponse ItensCriticosRecentes { get; set; } = new();
    public AdminDashboardAcoesRecomendadasResponse AcoesRecomendadas { get; set; } = new();
    public List<AdminDashboardProfissionalEmAtencaoItemResponse> TopProfissionaisEmAtencao { get; set; } = [];
    public List<AdminDashboardClienteEmAtencaoItemResponse> TopClientesEmAtencao { get; set; } = [];
    public List<AdminDashboardUsuarioInativoRecenteItemResponse> TopUsuariosInativosRecentes { get; set; } = [];
    public List<AdminDashboardAuditoriaAdminItemResponse> AcoesAdminRecentes { get; set; } = [];
    public List<AdminDashboardAdminAtivoItemResponse> TopAdminsAtivos { get; set; } = [];
    public AdminDashboardSlaOperacionalResponse SlaOperacional { get; set; } = new();
    public AdminDashboardDisponibilidadeOperacionalResponse DisponibilidadeOperacional { get; set; } = new();
    public AdminDashboardSaudeOperacionalResponse SaudeOperacional { get; set; } = new();
    public AdminDashboardResumoDecisorioResponse ResumoDecisorio { get; set; } = new();
}
