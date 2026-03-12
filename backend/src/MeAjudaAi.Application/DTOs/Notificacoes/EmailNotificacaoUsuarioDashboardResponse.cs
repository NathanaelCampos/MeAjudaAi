using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class EmailNotificacaoUsuarioDashboardResponse
{
    public Guid UsuarioId { get; set; }
    public TipoNotificacao? TipoNotificacao { get; set; }
    public DateTime? DataCriacaoInicial { get; set; }
    public DateTime? DataCriacaoFinal { get; set; }
    public EmailNotificacaoMetricasResponse Resumo { get; set; } = new();
    public EmailNotificacaoMetricasSerieResponse Serie { get; set; } = new();
    public EmailNotificacaoTiposMetricasResponse Tipos { get; set; } = new();
    public IReadOnlyList<EmailNotificacaoOutboxResponse> Recentes { get; set; } = Array.Empty<EmailNotificacaoOutboxResponse>();
}
