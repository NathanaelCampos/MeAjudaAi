using MeAjudaAi.Application.Interfaces.Notificacoes;
using MeAjudaAi.Application.Interfaces.Jobs;
using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Infrastructure.Configurations;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using MeAjudaAi.Infrastructure.Services.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Infrastructure.Services.Notificacoes;

public class EmailNotificacaoOutboxProcessor : ScheduledBackgroundJobProcessor<EmailNotificacaoOutboxProcessor>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<EmailNotificacaoOptions> _options;

    public EmailNotificacaoOutboxProcessor(
        IServiceScopeFactory scopeFactory,
        IOptions<EmailNotificacaoOptions> options,
        IBackgroundJobExecutionMetricsService metricsService,
        ILogger<EmailNotificacaoOutboxProcessor> logger)
        : base(metricsService, logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
    }

    public override string JobId => "emails-outbox";
    public override string Nome => "Processador do outbox de e-mails";
    public override bool Habilitado => _options.Value.ProcessadorHabilitado;
    public override int IntervaloSegundos => _options.Value.IntervaloSegundos;
    protected override int IntervaloMinimoSegundos => 5;
    protected override string MensagemDesabilitado => "Processador de e-mails do outbox desabilitado por configuração.";
    protected override string MensagemErro => "Erro ao processar lote de e-mails do outbox.";

    protected override async Task<int> ExecutarInternoAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sender = scope.ServiceProvider.GetRequiredService<IEmailNotificacaoSender>();
        var options = _options.Value;
        var lote = Math.Max(1, options.LoteProcessamento);
        var agora = DateTime.UtcNow;

        var emails = await context.EmailsNotificacoesOutbox
            .Where(x =>
                x.Ativo &&
                (x.Status == StatusEmailNotificacao.Pendente || x.Status == StatusEmailNotificacao.Falha) &&
                (x.ProximaTentativaEm == null || x.ProximaTentativaEm <= agora))
            .OrderBy(x => x.DataCriacao)
            .Take(lote)
            .ToListAsync(cancellationToken);

        if (emails.Count == 0)
            return 0;

        foreach (var email in emails)
        {
            email.TentativasProcessamento++;

            try
            {
                await sender.EnviarAsync(email, cancellationToken);
                email.Status = StatusEmailNotificacao.Enviado;
                email.DataProcessamento = agora;
                email.ProximaTentativaEm = null;
                email.UltimaMensagemErro = string.Empty;
                email.DataAtualizacao = agora;
            }
            catch (Exception ex)
            {
                email.DataProcessamento = agora;
                if (email.TentativasProcessamento >= Math.Max(1, options.MaxTentativas))
                {
                    email.Status = StatusEmailNotificacao.Cancelado;
                    email.ProximaTentativaEm = null;
                }
                else
                {
                    email.Status = StatusEmailNotificacao.Falha;
                    email.ProximaTentativaEm = agora.AddSeconds(Math.Max(5, options.AtrasoBaseSegundos) * email.TentativasProcessamento);
                }

                email.UltimaMensagemErro = ex.Message;
                email.DataAtualizacao = agora;
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return emails.Count;
    }
}
