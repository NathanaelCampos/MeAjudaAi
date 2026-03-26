using MeAjudaAi.Application.Interfaces.Admin;
using MeAjudaAi.Application.Interfaces.Jobs;
using MeAjudaAi.Application.Interfaces.Notificacoes;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Infrastructure.Configurations;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using MeAjudaAi.Infrastructure.Services.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Infrastructure.Services.Jobs;

public class AlertasFilaNotificationProcessor : ScheduledBackgroundJobProcessor<AlertasFilaNotificationProcessor>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly JobsAlertNotificationOptions _options;
    private Guid? _adminUsuarioId;

    public AlertasFilaNotificationProcessor(
        IServiceScopeFactory scopeFactory,
        IOptions<JobsAlertNotificationOptions> options,
        IBackgroundJobExecutionMetricsService metricsService,
        ILogger<AlertasFilaNotificationProcessor> logger)
        : base(metricsService, logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
    }

    public override string JobId => "alertas-fila-notificacoes";
    public override string Nome => "Notificações de alertas da fila";
    public override bool Habilitado => _options.Habilitado;
    public override int IntervaloSegundos => _options.IntervaloSegundos;
    protected override int IntervaloMinimoSegundos => 30;
    protected override string MensagemDesabilitado => "Notificações de alertas desabilitadas por configuração.";
    protected override string MensagemErro => "Erro ao processar alertas da fila para notificações.";

    protected override async Task<int> ExecutarInternoAsync(CancellationToken cancellationToken)
    {
        if (_options.NiveisParaNotificar is null || _options.NiveisParaNotificar.Length == 0)
            return 0;

        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (_adminUsuarioId is not null)
        {
            var adminAtivo = await context.Usuarios
                .AsNoTracking()
                .AnyAsync(x => x.Id == _adminUsuarioId && x.Ativo, cancellationToken);

            if (!adminAtivo)
                _adminUsuarioId = null;
        }

        if (_adminUsuarioId is null)
        {
            _adminUsuarioId = await ObterAdminUsuarioIdAsync(scope.ServiceProvider, cancellationToken);
            if (_adminUsuarioId is null)
                return 0;
        }

        var adminJobService = scope.ServiceProvider.GetRequiredService<IAdminJobService>();
        var notificacaoService = scope.ServiceProvider.GetRequiredService<INotificacaoService>();

        var alerts = await adminJobService.ObterAlertasFilaAsync(cancellationToken);
        if (alerts.Count == 0)
            return 0;

        var agora = DateTime.UtcNow;
        var notificacoesEnviadas = 0;

        foreach (var alert in alerts)
        {
            if (!NivelPermitido(alert.NivelAlerta))
                continue;

            var chave = $"{alert.JobId}:{alert.NivelAlerta}";
            var estado = await context.BackgroundJobFilaAlertasNotificacaoEstados
                .FirstOrDefaultAsync(x => x.Chave == chave, cancellationToken);

            if (estado != null && (agora - estado.UltimaNotificacaoEm).TotalMinutes < _options.IntervaloMinutosEntreNotificacoes)
                continue;

            var titulo = $"Alerta da fila: {alert.JobId}";
            var mensagem = $"{alert.NivelAlerta}: {alert.Mensagem} ({alert.TotalPendentes} pendentes, {alert.TotalFalhas} falhas).";

                await notificacaoService.CriarAsync(
                    _adminUsuarioId.Value,
                    TipoNotificacao.AlertaFila,
                    titulo,
                    mensagem,
                    cancellationToken: cancellationToken);

            if (estado is null)
            {
                estado = new BackgroundJobFilaAlertaNotificacaoEstado
                {
                    Chave = chave,
                    Ativo = true
                };

                context.BackgroundJobFilaAlertasNotificacaoEstados.Add(estado);
            }

            estado.UltimaNotificacaoEm = agora;

            notificacoesEnviadas++;
        }

        await context.SaveChangesAsync(cancellationToken);

        return notificacoesEnviadas;
    }

    private bool NivelPermitido(string nivel)
    {
        return _options.NiveisParaNotificar.Any(x => string.Equals(x, nivel, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<Guid?> ObterAdminUsuarioIdAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var context = services.GetRequiredService<AppDbContext>();
        return await context.Set<Usuario>()
            .Where(x => x.TipoPerfil == TipoPerfil.Administrador && x.Ativo)
            .OrderBy(x => x.DataCriacao)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
