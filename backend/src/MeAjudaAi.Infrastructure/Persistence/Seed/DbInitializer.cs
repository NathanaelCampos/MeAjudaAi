using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using MeAjudaAi.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace MeAjudaAi.Infrastructure.Persistence.Seed;

public static class DbInitializer
{
    public static async Task InicializarAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();
        await SeedProfissoesAsync(context);
        await SeedPlanosImpulsionamentoAsync(context);
        await SeedAdministradorAsync(context);
    }

    private static async Task SeedProfissoesAsync(AppDbContext context)
    {
        if (await context.Profissoes.AnyAsync())
            return;

        var profissaoEletricista = new Profissao
        {
            Nome = "Eletricista",
            Slug = "eletricista"
        };

        var profissaoEncanador = new Profissao
        {
            Nome = "Encanador",
            Slug = "encanador"
        };

        var profissaoFaxineira = new Profissao
        {
            Nome = "Faxineira",
            Slug = "faxineira"
        };

        var profissaoJardineiro = new Profissao
        {
            Nome = "Jardineiro",
            Slug = "jardineiro"
        };

        var profissaoPetSitter = new Profissao
        {
            Nome = "Pet Sitter",
            Slug = "pet-sitter"
        };

        await context.Profissoes.AddRangeAsync(
            profissaoEletricista,
            profissaoEncanador,
            profissaoFaxineira,
            profissaoJardineiro,
            profissaoPetSitter);

        await context.SaveChangesAsync();

        var especialidades = new List<Especialidade>
        {
            new() { ProfissaoId = profissaoEletricista.Id, Nome = "Eletricista Residencial" },
            new() { ProfissaoId = profissaoEletricista.Id, Nome = "Instalação Elétrica" },
            new() { ProfissaoId = profissaoEletricista.Id, Nome = "Manutenção Elétrica" },
            new() { ProfissaoId = profissaoEncanador.Id, Nome = "Encanador Residencial" },
            new() { ProfissaoId = profissaoEncanador.Id, Nome = "Reparo de Vazamentos" },
            new() { ProfissaoId = profissaoFaxineira.Id, Nome = "Faxina Residencial" },
            new() { ProfissaoId = profissaoFaxineira.Id, Nome = "Faxina Pós-Obra" },
            new() { ProfissaoId = profissaoJardineiro.Id, Nome = "Manutenção de Jardim" },
            new() { ProfissaoId = profissaoJardineiro.Id, Nome = "Poda" },
            new() { ProfissaoId = profissaoPetSitter.Id, Nome = "Cuidador de Pets" }
        };

        await context.Especialidades.AddRangeAsync(especialidades);
        await context.SaveChangesAsync();
    }

    private static async Task SeedAdministradorAsync(AppDbContext context)
    {
        const string emailAdmin = "admin@meajudaai.local";

        var adminExistente = await context.Usuarios
            .AnyAsync(x => x.Email == emailAdmin);

        if (adminExistente)
            return;

        var usuario = new Usuario
        {
            Nome = "Administrador",
            Email = emailAdmin,
            Telefone = string.Empty,
            TipoPerfil = TipoPerfil.Administrador,
            Ativo = true
        };

        var passwordHasher = new PasswordHasher<Usuario>();
        usuario.SenhaHash = passwordHasher.HashPassword(usuario, "Admin@123");

        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();
    }

    private static async Task SeedPlanosImpulsionamentoAsync(AppDbContext context)
    {
        if (await context.PlanosImpulsionamento.AnyAsync())
            return;

        var planos = new List<PlanoImpulsionamento>
    {
        new()
        {
            Nome = "Impulso 1 Dia",
            TipoPeriodo = TipoPeriodoImpulsionamento.Dia,
            QuantidadePeriodo = 1,
            Valor = 9.90m
        },
        new()
        {
            Nome = "Impulso 7 Dias",
            TipoPeriodo = TipoPeriodoImpulsionamento.Dia,
            QuantidadePeriodo = 7,
            Valor = 49.90m
        },
        new()
        {
            Nome = "Impulso 30 Dias",
            TipoPeriodo = TipoPeriodoImpulsionamento.Dia,
            QuantidadePeriodo = 30,
            Valor = 149.90m
        }
    };

        await context.PlanosImpulsionamento.AddRangeAsync(planos);
        await context.SaveChangesAsync();
    }
}