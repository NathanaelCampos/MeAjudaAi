namespace MeAjudaAi.Application.Interfaces.Notificacoes;

public interface INotificacaoRetentionService
{
    Task<int> ProcessarRetencaoAsync(CancellationToken cancellationToken = default);
}
