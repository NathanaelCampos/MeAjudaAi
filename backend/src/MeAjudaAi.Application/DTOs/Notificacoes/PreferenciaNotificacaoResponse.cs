using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class PreferenciaNotificacaoResponse
{
    public TipoNotificacao Tipo { get; set; }
    public bool AtivoInterno { get; set; }
}
