using MeAjudaAi.Domain.Common;

namespace MeAjudaAi.Domain.Entities;

public class BackgroundJobFilaAlertaNotificacaoEstado : EntityBase
{
    public string Chave { get; set; } = string.Empty;
    public DateTime UltimaNotificacaoEm { get; set; }
}
