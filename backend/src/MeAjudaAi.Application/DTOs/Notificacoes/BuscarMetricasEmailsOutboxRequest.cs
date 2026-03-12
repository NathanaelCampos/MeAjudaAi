using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class BuscarMetricasEmailsOutboxRequest
{
    public TipoNotificacao? TipoNotificacao { get; set; }
    public string? EmailDestino { get; set; }
    public DateTime? DataCriacaoInicial { get; set; }
    public DateTime? DataCriacaoFinal { get; set; }
}
