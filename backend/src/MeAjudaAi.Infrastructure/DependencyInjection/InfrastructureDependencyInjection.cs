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
using MeAjudaAi.Infrastructure.Services.Impulsionamentos;
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

        return services;
    }
}