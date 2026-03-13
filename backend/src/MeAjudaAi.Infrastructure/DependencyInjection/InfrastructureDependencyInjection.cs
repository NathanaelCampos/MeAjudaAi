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
using MeAjudaAi.Application.Interfaces.Notificacoes;
using MeAjudaAi.Infrastructure.Configurations;
using MeAjudaAi.Infrastructure.Services.Impulsionamentos;
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
        });

        services.AddScoped<IHashSenhaService, HashSenhaService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAdminUsuarioService, AdminUsuarioService>();
        services.AddScoped<IAdminAuditoriaService, AdminAuditoriaService>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();
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
        services.AddSingleton<INotificacaoRetentionMetricsService, NotificacaoRetentionMetricsService>();
        services.AddSingleton<NotificacaoInternaRetentionProcessor>();
        services.AddSingleton<INotificacaoRetentionService>(sp => sp.GetRequiredService<NotificacaoInternaRetentionProcessor>());
        services.AddHostedService<EmailNotificacaoOutboxProcessor>();
        services.AddHostedService(sp => sp.GetRequiredService<NotificacaoInternaRetentionProcessor>());

        return services;
    }
}
