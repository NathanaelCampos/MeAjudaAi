using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Application.Interfaces.Impulsionamentos;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MeAjudaAi.IntegrationTests.Infrastructure;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string EmailAdmin = "admin@teste.local";
    public const string SenhaAdmin = "Admin@123";

    private SqliteConnection _connection = null!;
    private string _contentRootPath = null!;

    public string ContentRootPath => _contentRootPath;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        _contentRootPath = Path.Combine(Path.GetTempPath(), "meajudaai-integration-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_contentRootPath);
        CopiarDadosCsv();
    }

    public new async Task DisposeAsync()
    {
        await _connection.DisposeAsync();

        if (Directory.Exists(_contentRootPath))
            Directory.Delete(_contentRootPath, recursive: true);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting(WebHostDefaults.ContentRootKey, _contentRootPath);

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DesabilitarDbInitializer"] = "true",
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Port=5432;Database=meajudaai_tests;Username=postgres;Password=postgres",
                ["Jwt:Issuer"] = "MeAjudaAi.Api.Tests",
                ["Jwt:Audience"] = "MeAjudaAi.Tests",
                ["Jwt:Key"] = "me-ajuda-ai-chave-de-teste-com-no-minimo-32-caracteres",
                ["Jwt:ExpiracaoEmMinutos"] = "120",
                ["Webhooks:Pagamentos:Segredo"] = "segredo-webhook-teste",
                ["Webhooks:Pagamentos:HeaderAssinatura"] = "X-Webhook-Signature",
                ["Webhooks:Pagamentos:Provedores:asaas:Segredo"] = "segredo-webhook-asaas-teste",
                ["Webhooks:Pagamentos:Provedores:asaas:HeaderAssinatura"] = "X-Asaas-Signature"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(_connection));

            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureCreated();

            SeedDadosBase(context);
        });
    }

    public async Task<Guid> ObterCidadeIdAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await context.Cidades
            .Select(x => x.Id)
            .FirstAsync();
    }

    public async Task<Guid> ObterProfissionalIdPorUsuarioIdAsync(Guid usuarioId)
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await context.Profissionais
            .Where(x => x.UsuarioId == usuarioId)
            .Select(x => x.Id)
            .FirstAsync();
    }

    public async Task<Guid> ObterBairroIdAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await context.Bairros
            .Select(x => x.Id)
            .FirstAsync();
    }

    public async Task<(Guid ProfissaoId, Guid EspecialidadeId)> ObterProfissaoEEspecialidadeAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var especialidade = await context.Especialidades
            .Select(x => new { x.ProfissaoId, x.Id })
            .FirstAsync();

        return (especialidade.ProfissaoId, especialidade.Id);
    }

    public async Task ResetStateAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        SeedDadosBase(context);

        var metricsService = scope.ServiceProvider.GetRequiredService<IWebhookPagamentoMetricsService>();
        metricsService.Reset();

        var uploadsPath = Path.Combine(_contentRootPath, "Uploads");
        if (Directory.Exists(uploadsPath))
            Directory.Delete(uploadsPath, recursive: true);
    }

    private static void SeedDadosBase(AppDbContext context)
    {
        if (!context.Estados.Any())
        {
            var estado = new Estado
            {
                Nome = "Sao Paulo",
                UF = "SP",
                CodigoIbge = "35"
            };

            context.Estados.Add(estado);
            context.SaveChanges();

            context.Cidades.Add(new Cidade
            {
                EstadoId = estado.Id,
                Nome = "Sao Paulo",
                CodigoIbge = "3550308"
            });

            context.SaveChanges();

            var cidadeId = context.Cidades.Select(x => x.Id).First();

            context.Bairros.Add(new Bairro
            {
                CidadeId = cidadeId,
                Nome = "Centro"
            });

            context.SaveChanges();
        }

        if (!context.Profissoes.Any())
        {
            var profissao = new Profissao
            {
                Nome = "Eletricista",
                Slug = "eletricista"
            };

            context.Profissoes.Add(profissao);
            context.SaveChanges();

            context.Especialidades.Add(new Especialidade
            {
                ProfissaoId = profissao.Id,
                Nome = "Instalacao Residencial"
            });

            context.SaveChanges();
        }

        if (!context.PlanosImpulsionamento.Any())
        {
            context.PlanosImpulsionamento.AddRange(
                new PlanoImpulsionamento
                {
                    Nome = "Impulso 1 Dia",
                    TipoPeriodo = TipoPeriodoImpulsionamento.Dia,
                    QuantidadePeriodo = 1,
                    Valor = 9.90m
                },
                new PlanoImpulsionamento
                {
                    Nome = "Impulso 7 Dias",
                    TipoPeriodo = TipoPeriodoImpulsionamento.Dia,
                    QuantidadePeriodo = 7,
                    Valor = 49.90m
                });

            context.SaveChanges();
        }

        if (!context.Usuarios.Any(x => x.Email == EmailAdmin))
        {
            var admin = new Usuario
            {
                Nome = "Administrador Teste",
                Email = EmailAdmin,
                Telefone = string.Empty,
                TipoPerfil = TipoPerfil.Administrador,
                Ativo = true
            };

            var passwordHasher = new PasswordHasher<Usuario>();
            admin.SenhaHash = passwordHasher.HashPassword(admin, SenhaAdmin);

            context.Usuarios.Add(admin);
            context.SaveChanges();
        }
    }

    private void CopiarDadosCsv()
    {
        var origemDadosCsv = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../src/MeAjudaAi.Api/DadosCsv"));

        var destinoDadosCsv = Path.Combine(_contentRootPath, "DadosCsv");
        Directory.CreateDirectory(destinoDadosCsv);

        foreach (var arquivo in Directory.GetFiles(origemDadosCsv, "*.csv"))
        {
            var destino = Path.Combine(destinoDadosCsv, Path.GetFileName(arquivo));
            File.Copy(arquivo, destino, overwrite: true);
        }
    }
}
