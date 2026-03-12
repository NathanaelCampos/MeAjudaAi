using MeAjudaAi.Application.Interfaces.Notificacoes;
using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Infrastructure.Services.Notificacoes;

public class EmailNotificacaoSender : IEmailNotificacaoSender
{
    private readonly EmailNotificacaoOptions _options;
    private readonly FakeEmailNotificacaoSender _fakeSender;
    private readonly SmtpEmailNotificacaoSender _smtpSender;

    public EmailNotificacaoSender(
        IOptions<EmailNotificacaoOptions> options,
        FakeEmailNotificacaoSender fakeSender,
        SmtpEmailNotificacaoSender smtpSender)
    {
        _options = options.Value;
        _fakeSender = fakeSender;
        _smtpSender = smtpSender;
    }

    public Task EnviarAsync(EmailNotificacaoOutbox email, CancellationToken cancellationToken = default)
    {
        return _options.SimularEnvio
            ? _fakeSender.EnviarAsync(email, cancellationToken)
            : _smtpSender.EnviarAsync(email, cancellationToken);
    }
}
