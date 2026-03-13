using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Application.Interfaces.Notificacoes;
using MeAjudaAi.Infrastructure.Configurations;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Infrastructure.Services.Notificacoes;

public class NotificacaoInternaRetentionProcessor : BackgroundService, INotificacaoRetentionService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<NotificacaoInternaRetentionOptions> _options;
    private readonly ILogger<NotificacaoInternaRetentionProcessor> _logger;

    public NotificacaoInternaRetentionProcessor(
        IServiceScopeFactory scopeFactory,
        IOptions<NotificacaoInternaRetentionOptions> options,
        ILogger<NotificacaoInternaRetentionProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = _options.Value;

        if (!config.Habilitada)
        {
            _logger.LogInformation("Retenção automática de notificações internas desabilitada por configuração.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessarRetencaoAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar retenção de notificações internas.");
            }

            await Task.Delay(TimeSpan.FromSeconds(Math.Max(60, config.IntervaloSegundos)), stoppingToken);
        }
    }

    public async Task<int> ProcessarRetencaoAsync(CancellationToken cancellationToken = default)
    {
        var options = _options.Value;

        if (!options.Habilitada || options.DiasRetencao <= 0)
            return 0;

        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var agora = DateTime.UtcNow;
        var dataLimite = agora.AddDays(-options.DiasRetencao);

        var query = context.Set<NotificacaoUsuario>()
            .Where(x => x.Ativo && x.DataCriacao <= dataLimite);

        if (options.SomenteLidas)
            query = query.Where(x => x.DataLeitura != null);

        var notificacoes = await query
            .OrderBy(x => x.DataCriacao)
            .Take(Math.Max(1, options.LoteProcessamento))
            .ToListAsync(cancellationToken);

        if (notificacoes.Count == 0)
            return 0;

        foreach (var notificacao in notificacoes)
        {
            notificacao.Ativo = false;
            notificacao.DataAtualizacao = agora;
        }

        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Retenção de notificações internas arquivou {Quantidade} registros com corte em {DataLimite}.",
            notificacoes.Count,
            dataLimite);

        return notificacoes.Count;
    }
}
