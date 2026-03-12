using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Infrastructure.Configurations;
using MeAjudaAi.Infrastructure.Services.Notificacoes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.UnitTests.Notificacoes;

public class EmailNotificacaoSenderTests
{
    [Fact]
    public async Task EnviarAsync_ComModoSimulado_DeveUsarSenderFake()
    {
        var options = Options.Create(new EmailNotificacaoOptions
        {
            SimularEnvio = true
        });

        var sender = new EmailNotificacaoSender(
            options,
            new FakeEmailNotificacaoSender(NullLogger<FakeEmailNotificacaoSender>.Instance, options),
            new SmtpEmailNotificacaoSender(NullLogger<SmtpEmailNotificacaoSender>.Instance, options));

        var email = CriarEmail();

        await sender.EnviarAsync(email);
    }

    [Fact]
    public async Task EnviarAsync_ComSmtpSemHost_DeveFalhar()
    {
        var options = Options.Create(new EmailNotificacaoOptions
        {
            SimularEnvio = false,
            RemetenteEmail = "noreply@teste.local"
        });

        var sender = new EmailNotificacaoSender(
            options,
            new FakeEmailNotificacaoSender(NullLogger<FakeEmailNotificacaoSender>.Instance, options),
            new SmtpEmailNotificacaoSender(NullLogger<SmtpEmailNotificacaoSender>.Instance, options));

        var email = CriarEmail();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => sender.EnviarAsync(email));
        Assert.Equal("SMTP host não configurado.", exception.Message);
    }

    private static EmailNotificacaoOutbox CriarEmail()
    {
        return new EmailNotificacaoOutbox
        {
            UsuarioId = Guid.NewGuid(),
            TipoNotificacao = TipoNotificacao.ServicoSolicitado,
            EmailDestino = "destinatario@teste.local",
            Assunto = "Teste",
            Corpo = "Conteudo"
        };
    }
}
