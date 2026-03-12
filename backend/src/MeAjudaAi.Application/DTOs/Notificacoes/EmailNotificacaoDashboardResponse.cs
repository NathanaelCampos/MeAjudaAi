using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class EmailNotificacaoDashboardResponse
{
    public TipoNotificacao? TipoNotificacao { get; set; }
    public string? EmailDestino { get; set; }
    public DateTime? DataCriacaoInicial { get; set; }
    public DateTime? DataCriacaoFinal { get; set; }
    public EmailNotificacaoMetricasResponse Resumo { get; set; } = new();
    public EmailNotificacaoMetricasSerieResponse Serie { get; set; } = new();
    public EmailNotificacaoDestinatariosMetricasResponse Destinatarios { get; set; } = new();
    public EmailNotificacaoTiposMetricasResponse Tipos { get; set; } = new();
}
