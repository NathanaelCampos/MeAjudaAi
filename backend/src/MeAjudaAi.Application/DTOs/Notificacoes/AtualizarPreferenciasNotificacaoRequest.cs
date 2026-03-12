namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class AtualizarPreferenciasNotificacaoRequest
{
    public IReadOnlyList<PreferenciaNotificacaoItemRequest> Preferencias { get; set; } = Array.Empty<PreferenciaNotificacaoItemRequest>();
}
