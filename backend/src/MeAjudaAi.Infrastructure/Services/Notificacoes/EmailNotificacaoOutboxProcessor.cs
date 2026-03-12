using MeAjudaAi.Application.Interfaces.Notificacoes;
using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Infrastructure.Configurations;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Infrastructure.Services.Notificacoes;

public class EmailNotificacaoOutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<EmailNotificacaoOptions> _options;
    private readonly ILogger<EmailNotificacaoOutboxProcessor> _logger;

    public EmailNotificacaoOutboxProcessor(
        IServiceScopeFactory scopeFactory,
        IOptions<EmailNotificacaoOptions> options,
        ILogger<EmailNotificacaoOutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = _options.Value;

        if (!config.ProcessadorHabilitado)
        {
            _logger.LogInformation("Processador de e-mails do outbox desabilitado por configuração.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessarLoteAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar lote de e-mails do outbox.");
            }

            await Task.Delay(TimeSpan.FromSeconds(Math.Max(5, config.IntervaloSegundos)), stoppingToken);
        }
    }

    public async Task<int> ProcessarLoteAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sender = scope.ServiceProvider.GetRequiredService<IEmailNotificacaoSender>();
        var lote = Math.Max(1, _options.Value.LoteProcessamento);

        var emails = await context.EmailsNotificacoesOutbox
            .Where(x => x.Ativo && (x.Status == StatusEmailNotificacao.Pendente || x.Status == StatusEmailNotificacao.Falha))
            .OrderBy(x => x.DataCriacao)
            .Take(lote)
            .ToListAsync(cancellationToken);

        if (emails.Count == 0)
            return 0;

        var agora = DateTime.UtcNow;

        foreach (var email in emails)
        {
            try
            {
                await sender.EnviarAsync(email, cancellationToken);
                email.Status = StatusEmailNotificacao.Enviado;
                email.DataProcessamento = agora;
                email.UltimaMensagemErro = string.Empty;
                email.DataAtualizacao = agora;
            }
            catch (Exception ex)
            {
                email.Status = StatusEmailNotificacao.Falha;
                email.DataProcessamento = agora;
                email.UltimaMensagemErro = ex.Message;
                email.DataAtualizacao = agora;
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return emails.Count;
    }
}
