using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class EmailNotificacaoMetricaSerieItemResponse
{
    public DateTime Data { get; set; }
    public TipoNotificacao TipoNotificacao { get; set; }
    public StatusEmailNotificacao Status { get; set; }
    public int Quantidade { get; set; }
}
