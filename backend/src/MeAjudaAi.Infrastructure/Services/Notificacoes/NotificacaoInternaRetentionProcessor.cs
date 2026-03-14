using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Application.Interfaces.Jobs;
using MeAjudaAi.Application.Interfaces.Notificacoes;
using MeAjudaAi.Infrastructure.Configurations;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using MeAjudaAi.Infrastructure.Services.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Infrastructure.Services.Notificacoes;

public class NotificacaoInternaRetentionProcessor : ScheduledBackgroundJobProcessor<NotificacaoInternaRetentionProcessor>, INotificacaoRetentionService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<NotificacaoInternaRetentionOptions> _options;
    private readonly INotificacaoRetentionMetricsService _metricsService;

    public NotificacaoInternaRetentionProcessor(
        IServiceScopeFactory scopeFactory,
        IOptions<NotificacaoInternaRetentionOptions> options,
        IBackgroundJobExecutionMetricsService backgroundJobMetricsService,
        INotificacaoRetentionMetricsService metricsService,
        ILogger<NotificacaoInternaRetentionProcessor> logger)
        : base(backgroundJobMetricsService, logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _metricsService = metricsService;
    }

    public override string JobId => "notificacoes-retencao";
    public override string Nome => "Retenção de notificações internas";
    public override bool Habilitado => _options.Value.Habilitada;
    public override int IntervaloSegundos => _options.Value.IntervaloSegundos;
    protected override int IntervaloMinimoSegundos => 60;
    protected override string MensagemDesabilitado => "Retenção automática de notificações internas desabilitada por configuração.";
    protected override string MensagemErro => "Erro ao processar retenção de notificações internas.";

    public Task<int> ProcessarRetencaoAsync(CancellationToken cancellationToken = default)
    {
        return ExecutarAsync(cancellationToken);
    }

    protected override async Task<int> ExecutarInternoAsync(CancellationToken cancellationToken)
    {
        var options = _options.Value;
        var iniciadoEm = DateTime.UtcNow;

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

        _metricsService.RegistrarSucesso(agora, notificacoes.Count);

        return notificacoes.Count;
    }
}
