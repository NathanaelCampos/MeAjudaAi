using MeAjudaAi.Application.Interfaces.Auth;
using MeAjudaAi.Application.Interfaces.Profissionais;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using MeAjudaAi.Infrastructure.Services.Auth;
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
            options.SimularEnvio = bool.TryParse(emailSection["SimularEnvio"], out var simularEnvio)
                ? simularEnvio
                : true;
        });

        services.AddScoped<IHashSenhaService, HashSenhaService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProfissionalService, ProfissionalService>();
        services.AddScoped<IProfissaoService, ProfissaoService>();
        services.AddScoped<ICidadeService, CidadeService>();
        services.AddScoped<IAvaliacaoService, AvaliacaoService>();
        services.AddScoped<IServicoService, ServicoService>();
        services.AddScoped<ImportadorGeografiaService>();
        services.AddScoped<IArquivoStorageService, ArquivoStorageService>();
        services.AddScoped<IImpulsionamentoService, ImpulsionamentoService>();
        services.AddScoped<INotificacaoService, NotificacaoService>();
        services.AddScoped<IEmailNotificacaoSender, FakeEmailNotificacaoSender>();
        services.AddSingleton<IWebhookPagamentoMetricsService, WebhookPagamentoMetricsService>();
        services.AddHostedService<EmailNotificacaoOutboxProcessor>();

        return services;
    }
}
