using MeAjudaAi.Application.Interfaces.Notificacoes;
using MeAjudaAi.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MeAjudaAi.Infrastructure.Configurations;

namespace MeAjudaAi.Infrastructure.Services.Notificacoes;

public class FakeEmailNotificacaoSender : IEmailNotificacaoSender
{
    private readonly ILogger<FakeEmailNotificacaoSender> _logger;
    private readonly EmailNotificacaoOptions _options;

    public FakeEmailNotificacaoSender(
        ILogger<FakeEmailNotificacaoSender> logger,
        IOptions<EmailNotificacaoOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public Task EnviarAsync(EmailNotificacaoOutbox email, CancellationToken cancellationToken = default)
    {
        if (!_options.SimularEnvio)
            throw new InvalidOperationException("Envio real de e-mail não configurado.");

        _logger.LogInformation(
            "E-mail de notificação simulado para {EmailDestino} tipo {TipoNotificacao} referência {ReferenciaId}",
            email.EmailDestino,
            email.TipoNotificacao,
            email.ReferenciaId);

        return Task.CompletedTask;
    }
}
