using MeAjudaAi.Application.Interfaces.Auth;
using MeAjudaAi.Application.Interfaces.Admin;
using MeAjudaAi.Application.Interfaces.Profissionais;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using MeAjudaAi.Infrastructure.Services.Auth;
using MeAjudaAi.Infrastructure.Services.Admin;
using MeAjudaAi.Infrastructure.Services.Profissionais;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MeAjudaAi.Application.Interfaces.Cidades;
using MeAjudaAi.Application.Interfaces.Profissoes;
using Microsoft.Extensions.Configuration;
using MeAjudaAi.Infrastructure.Services.Cidades;
using MeAjudaAi.Infrastructure.Services.Profissoes;
using MeAjudaAi.Application.Interfaces.Avaliacoes;
using MeAjudaAi.Infrastructure.Services.Avaliacoes;
using MeAjudaAi.Application.Interfaces.Servicos;
using MeAjudaAi.Infrastructure.Services.Servicos;
using MeAjudaAi.Infrastructure.Importacao;
using MeAjudaAi.Application.Interfaces.Storage;
using MeAjudaAi.Infrastructure.Services.Storage;
using MeAjudaAi.Application.Interfaces.Impulsionamentos;
using MeAjudaAi.Application.Interfaces.Jobs;
using MeAjudaAi.Application.Interfaces.Notificacoes;
using MeAjudaAi.Infrastructure.Configurations;
using MeAjudaAi.Infrastructure.Services.Impulsionamentos;
using MeAjudaAi.Infrastructure.Services.Jobs;
using MeAjudaAi.Infrastructure.Services.Notificacoes;
namespace MeAjudaAi.Infrastructure.DependencyInjection;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        var emailSection = configuration.GetSection("Emails:Notificacoes");
        services.Configure<EmailNotificacaoOptions>(options =>
        {
            options.ProcessadorHabilitado = bool.TryParse(emailSection["ProcessadorHabilitado"], out var processadorHabilitado)
                ? processadorHabilitado
                : false;
            options.LoteProcessamento = int.TryParse(emailSection["LoteProcessamento"], out var loteProcessamento)
                ? loteProcessamento
                : 20;
            options.IntervaloSegundos = int.TryParse(emailSection["IntervaloSegundos"], out var intervaloSegundos)
                ? intervaloSegundos
                : 60;
            options.AtrasoBaseSegundos = int.TryParse(emailSection["AtrasoBaseSegundos"], out var atrasoBaseSegundos)
                ? atrasoBaseSegundos
                : 60;
            options.MaxTentativas = int.TryParse(emailSection["MaxTentativas"], out var maxTentativas)
                ? maxTentativas
                : 3;
            options.SimularEnvio = bool.TryParse(emailSection["SimularEnvio"], out var simularEnvio)
                ? simularEnvio
                : true;
            options.RemetenteEmail = emailSection["RemetenteEmail"] ?? string.Empty;
            options.RemetenteNome = emailSection["RemetenteNome"] ?? string.Empty;
            options.SmtpHost = emailSection["SmtpHost"] ?? string.Empty;
            options.SmtpPort = int.TryParse(emailSection["SmtpPort"], out var smtpPort)
                ? smtpPort
                : 25;
            options.SmtpUsuario = emailSection["SmtpUsuario"] ?? string.Empty;
            options.SmtpSenha = emailSection["SmtpSenha"] ?? string.Empty;
            options.SmtpSsl = bool.TryParse(emailSection["SmtpSsl"], out var smtpSsl)
                ? smtpSsl
                : true;
        });

        var retencaoSection = configuration.GetSection("Notificacoes:Internas:Retencao");
        services.Configure<NotificacaoInternaRetentionOptions>(options =>
        {
            options.Habilitada = bool.TryParse(retencaoSection["Habilitada"], out var habilitada)
                ? habilitada
                : false;
            options.DiasRetencao = int.TryParse(retencaoSection["DiasRetencao"], out var diasRetencao)
                ? diasRetencao
                : 30;
            options.LoteProcessamento = int.TryParse(retencaoSection["LoteProcessamento"], out var loteRetencao)
                ? loteRetencao
                : 500;
            options.IntervaloSegundos = int.TryParse(retencaoSection["IntervaloSegundos"], out var intervaloRetencao)
                ? intervaloRetencao
                : 3600;
            options.SomenteLidas = bool.TryParse(retencaoSection["SomenteLidas"], out var somenteLidas)
                ? somenteLidas
                : true;
        });

        var adminDashboardSection = configuration.GetSection("Admin:Dashboard");
        services.Configure<AdminDashboardOptions>(options =>
        {
            options.JanelaQualidadeDias = int.TryParse(adminDashboardSection["JanelaQualidadeDias"], out var janelaQualidadeDias)
                ? Math.Max(janelaQualidadeDias, 1)
                : 7;
            options.JanelaAcaoAdminRecenteHoras = int.TryParse(adminDashboardSection["JanelaAcaoAdminRecenteHoras"], out var janelaAcaoAdminRecenteHoras)
                ? Math.Max(janelaAcaoAdminRecenteHoras, 1)
                : 24;
            options.JanelaSerieDias = int.TryParse(adminDashboardSection["JanelaSerieDias"], out var janelaSerieDias)
                ? Math.Max(janelaSerieDias, 1)
                : 7;
        });

        var jobsQueueSection = configuration.GetSection("Jobs:Fila");
        services.Configure<BackgroundJobQueueOptions>(options =>
        {
            options.Habilitada = bool.TryParse(jobsQueueSection["Habilitada"], out var habilitada)
                ? habilitada
                : false;
            options.IntervaloSegundos = int.TryParse(jobsQueueSection["IntervaloSegundos"], out var intervaloSegundos)
                ? Math.Max(intervaloSegundos, 5)
                : 30;
            options.LoteProcessamento = int.TryParse(jobsQueueSection["LoteProcessamento"], out var loteProcessamento)
                ? Math.Max(loteProcessamento, 1)
                : 20;
            options.MaxTentativas = int.TryParse(jobsQueueSection["MaxTentativas"], out var maxTentativas)
                ? Math.Max(maxTentativas, 1)
                : 3;
            options.AtrasoBaseSegundos = int.TryParse(jobsQueueSection["AtrasoBaseSegundos"], out var atrasoBaseSegundos)
                ? Math.Max(atrasoBaseSegundos, 5)
                : 60;
        });

        var jobsAlertSection = configuration.GetSection("Jobs:Alertas");
        services.Configure<JobsAlertOptions>(options =>
        {
            options.Habilitado = bool.TryParse(jobsAlertSection["Habilitado"], out var habilitado)
                ? habilitado
                : true;
            options.TempoEsperaLimiteSegundos = double.TryParse(jobsAlertSection["TempoEsperaLimiteSegundos"], out var espera)
                ? Math.Max(espera, 1)
                : 30;
            options.TempoProcessamentoLimiteSegundos = double.TryParse(jobsAlertSection["TempoProcessamentoLimiteSegundos"], out var processamento)
                ? Math.Max(processamento, 1)
                : 30;
        });

        var jobsAlertNotificationSection = configuration.GetSection("Jobs:Alertas:Notificacoes");
        services.Configure<JobsAlertNotificationOptions>(options =>
        {
            options.Habilitado = bool.TryParse(jobsAlertNotificationSection["Habilitado"], out var habilitadoNotification)
                ? habilitadoNotification
                : true;
            options.IntervaloSegundos = int.TryParse(jobsAlertNotificationSection["IntervaloSegundos"], out var intervaloNotification)
                ? Math.Max(intervaloNotification, 30)
                : 60;
            options.IntervaloMinutosEntreNotificacoes = int.TryParse(jobsAlertNotificationSection["IntervaloMinutosEntreNotificacoes"], out var intervaloMinutos)
                ? Math.Max(intervaloMinutos, 1)
                : 30;

            var niveisSection = jobsAlertNotificationSection.GetSection("NiveisParaNotificar");
            var niveis = niveisSection.Exists()
                ? niveisSection.Get<string[]>()
                : null;

            if (niveis is { Length: > 0 })
                options.NiveisParaNotificar = niveis;
        });

        var jobsWatchdogSection = configuration.GetSection("Jobs:Watchdog");
        services.Configure<BackgroundJobWatchdogOptions>(options =>
        {
            options.Habilitado = bool.TryParse(jobsWatchdogSection["Habilitado"], out var habilitadoWatchdog)
                ? habilitadoWatchdog
                : true;
            options.IntervaloSegundos = int.TryParse(jobsWatchdogSection["IntervaloSegundos"], out var intervaloWatchdog)
                ? Math.Max(intervaloWatchdog, 30)
                : 60;
            options.TempoMaximoProcessandoSegundos = int.TryParse(jobsWatchdogSection["TempoMaximoProcessandoSegundos"], out var tempoMaximo)
                ? Math.Max(tempoMaximo, 60)
                : 300;
        });

        var jobsRetrySection = configuration.GetSection("Jobs:Retry");
        services.Configure<BackgroundJobRetrySchedulerOptions>(options =>
        {
            options.Habilitado = bool.TryParse(jobsRetrySection["Habilitado"], out var habilitadoRetry)
                ? habilitadoRetry
                : true;
            options.IntervaloSegundos = int.TryParse(jobsRetrySection["IntervaloSegundos"], out var intervaloRetry)
                ? Math.Max(intervaloRetry, 60)
                : 120;
            options.FalhasParaRetentar = int.TryParse(jobsRetrySection["FalhasParaRetentar"], out var falhas)
                ? Math.Max(falhas, 1)
                : 1;
            options.TempoMaximoDesdeFalhaSegundos = int.TryParse(jobsRetrySection["TempoMaximoDesdeFalhaSegundos"], out var tempoMaximo)
                ? Math.Max(tempoMaximo, 60)
                : 300;
        });

        services.AddScoped<IHashSenhaService, HashSenhaService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAdminUsuarioService, AdminUsuarioService>();
        services.AddScoped<IAdminAuditoriaService, AdminAuditoriaService>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();
        services.AddScoped<IAdminJobService, AdminJobService>();
        services.AddScoped<IAdminProfissionalService, AdminProfissionalService>();
        services.AddScoped<IAdminServicoService, AdminServicoService>();
        services.AddScoped<IAdminAvaliacaoService, AdminAvaliacaoService>();
        services.AddScoped<IAdminImpulsionamentoService, AdminImpulsionamentoService>();
        services.AddScoped<IAdminWebhookPagamentoService, AdminWebhookPagamentoService>();
        services.AddScoped<IProfissionalService, ProfissionalService>();
        services.AddScoped<IProfissaoService, ProfissaoService>();
        services.AddScoped<ICidadeService, CidadeService>();
        services.AddScoped<IAvaliacaoService, AvaliacaoService>();
        services.AddScoped<IServicoService, ServicoService>();
        services.AddScoped<ImportadorGeografiaService>();
        services.AddScoped<IArquivoStorageService, ArquivoStorageService>();
        services.AddScoped<IImpulsionamentoService, ImpulsionamentoService>();
        services.AddScoped<INotificacaoService, NotificacaoService>();
        services.AddScoped<IEmailNotificacaoTemplateRenderer, EmailNotificacaoTemplateRenderer>();
        services.AddScoped<FakeEmailNotificacaoSender>();
        services.AddScoped<SmtpEmailNotificacaoSender>();
        services.AddScoped<IEmailNotificacaoSender, EmailNotificacaoSender>();
        services.AddSingleton<IWebhookPagamentoMetricsService, WebhookPagamentoMetricsService>();
        services.AddSingleton<NotificacaoRetentionMetricsService>();
        services.AddSingleton<INotificacaoRetentionMetricsService>(sp => sp.GetRequiredService<NotificacaoRetentionMetricsService>());
        services.AddSingleton<IBackgroundJobExecutionMetricsService, BackgroundJobExecutionMetricsService>();
        services.AddSingleton<BackgroundJobQueueProcessor>();
        services.AddSingleton<IBackgroundJobQueueProcessor>(sp => sp.GetRequiredService<BackgroundJobQueueProcessor>());
        services.AddSingleton<EmailNotificacaoOutboxProcessor>();
        services.AddSingleton<IBackgroundJobProcessor>(sp => sp.GetRequiredService<EmailNotificacaoOutboxProcessor>());
        services.AddSingleton<NotificacaoInternaRetentionProcessor>();
        services.AddSingleton<IBackgroundJobProcessor>(sp => sp.GetRequiredService<NotificacaoInternaRetentionProcessor>());
        services.AddSingleton<AlertasFilaNotificationProcessor>();
        services.AddSingleton<IBackgroundJobProcessor>(sp => sp.GetRequiredService<AlertasFilaNotificationProcessor>());
        services.AddSingleton<BackgroundJobWatchdogProcessor>();
        services.AddSingleton<IBackgroundJobProcessor>(sp => sp.GetRequiredService<BackgroundJobWatchdogProcessor>());
        services.AddSingleton<BackgroundJobRetryScheduler>();
        services.AddSingleton<IBackgroundJobProcessor>(sp => sp.GetRequiredService<BackgroundJobRetryScheduler>());
        services.AddHostedService(sp => sp.GetRequiredService<AlertasFilaNotificationProcessor>());
        services.AddHostedService(sp => sp.GetRequiredService<BackgroundJobWatchdogProcessor>());
        services.AddHostedService(sp => sp.GetRequiredService<BackgroundJobRetryScheduler>());
        services.AddSingleton<INotificacaoRetentionService>(sp => sp.GetRequiredService<NotificacaoInternaRetentionProcessor>());
        services.AddHostedService(sp => sp.GetRequiredService<BackgroundJobQueueProcessor>());
        services.AddHostedService(sp => sp.GetRequiredService<EmailNotificacaoOutboxProcessor>());
        services.AddHostedService(sp => sp.GetRequiredService<NotificacaoInternaRetentionProcessor>());

        return services;
    }
}
