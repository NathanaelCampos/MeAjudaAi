using MeAjudaAi.Domain.Entities;

namespace MeAjudaAi.Application.Interfaces.Notificacoes;

public interface IEmailNotificacaoSender
{
    Task EnviarAsync(EmailNotificacaoOutbox email, CancellationToken cancellationToken = default);
}
