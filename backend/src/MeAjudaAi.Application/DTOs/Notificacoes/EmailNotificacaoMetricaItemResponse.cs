using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class EmailNotificacaoMetricaItemResponse
{
    public StatusEmailNotificacao Status { get; set; }
    public int Quantidade { get; set; }
}
