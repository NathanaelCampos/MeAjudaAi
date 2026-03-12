using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class EmailNotificacaoTipoMetricaItemResponse
{
    public TipoNotificacao TipoNotificacao { get; set; }
    public int Total { get; set; }
    public int Pendentes { get; set; }
    public int Enviados { get; set; }
    public int Falhas { get; set; }
    public int Cancelados { get; set; }
}
