using System.Net;
using System.Net.Mail;
using MeAjudaAi.Application.Interfaces.Notificacoes;
using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Infrastructure.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Infrastructure.Services.Notificacoes;

public class SmtpEmailNotificacaoSender : IEmailNotificacaoSender
{
    private readonly ILogger<SmtpEmailNotificacaoSender> _logger;
    private readonly EmailNotificacaoOptions _options;
    private readonly IEmailNotificacaoTemplateRenderer _templateRenderer;

    public SmtpEmailNotificacaoSender(
        ILogger<SmtpEmailNotificacaoSender> logger,
        IOptions<EmailNotificacaoOptions> options,
        IEmailNotificacaoTemplateRenderer templateRenderer)
    {
        _logger = logger;
        _options = options.Value;
        _templateRenderer = templateRenderer;
    }

    public async Task EnviarAsync(EmailNotificacaoOutbox email, CancellationToken cancellationToken = default)
    {
        ValidarConfiguracao();

        using var message = new MailMessage
        {
            From = new MailAddress(_options.RemetenteEmail, _options.RemetenteNome),
            Subject = email.Assunto,
            Body = _templateRenderer.RenderizarHtml(email),
            IsBodyHtml = true
        };

        message.To.Add(email.EmailDestino);

        using var smtpClient = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.SmtpSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrWhiteSpace(_options.SmtpUsuario))
        {
            smtpClient.Credentials = new NetworkCredential(_options.SmtpUsuario, _options.SmtpSenha);
        }

        _logger.LogInformation(
            "Enviando e-mail SMTP para {EmailDestino} tipo {TipoNotificacao} referência {ReferenciaId}",
            email.EmailDestino,
            email.TipoNotificacao,
            email.ReferenciaId);

        cancellationToken.ThrowIfCancellationRequested();
        await smtpClient.SendMailAsync(message, cancellationToken);
    }

    private void ValidarConfiguracao()
    {
        if (string.IsNullOrWhiteSpace(_options.SmtpHost))
            throw new InvalidOperationException("SMTP host não configurado.");

        if (string.IsNullOrWhiteSpace(_options.RemetenteEmail))
            throw new InvalidOperationException("Remetente de e-mail não configurado.");
    }
}
