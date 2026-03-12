using MeAjudaAi.Domain.Entities;

namespace MeAjudaAi.Application.Interfaces.Notificacoes;

public interface IEmailNotificacaoTemplateRenderer
{
    string RenderizarHtml(EmailNotificacaoOutbox email);
}
