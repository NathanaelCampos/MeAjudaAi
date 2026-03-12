using MeAjudaAi.Application.DTOs.Impulsionamentos;
using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.Data.Sqlite;
using MeAjudaAi.Infrastructure.Services.Impulsionamentos;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.UnitTests.Impulsionamentos;

public class ImpulsionamentoServiceTests
{
    [Fact]
    public async Task ContratarPlanoAsync_DeveAgendarNovoImpulsionamentoAposOFimDoAtual()
    {
        await using var context = CriarContexto();

        var usuarioId = Guid.NewGuid();
        var usuario = new Usuario
        {
            Id = usuarioId,
            Nome = "Profissional Teste",
            Email = "profissional1@teste.local",
            Telefone = string.Empty,
            SenhaHash = "hash",
            TipoPerfil = TipoPerfil.Profissional
        };
        var profissional = new Profissional
        {
            UsuarioId = usuarioId,
            NomeExibicao = "Profissional Teste"
        };

        var planoAtual = new PlanoImpulsionamento
        {
            Nome = "Plano Atual",
            TipoPeriodo = TipoPeriodoImpulsionamento.Dia,
            QuantidadePeriodo = 2,
            Valor = 20m
        };

        var novoPlano = new PlanoImpulsionamento
        {
            Nome = "Novo Plano",
            TipoPeriodo = TipoPeriodoImpulsionamento.Dia,
            QuantidadePeriodo = 7,
            Valor = 70m
        };

        var dataFimAtual = DateTime.UtcNow.AddDays(2);

        context.Usuarios.Add(usuario);
        context.Profissionais.Add(profissional);
        context.PlanosImpulsionamento.AddRange(planoAtual, novoPlano);
        context.ImpulsionamentosProfissionais.Add(new ImpulsionamentoProfissional
        {
            ProfissionalId = profissional.Id,
            PlanoImpulsionamentoId = planoAtual.Id,
            DataInicio = DateTime.UtcNow.AddDays(-1),
            DataFim = dataFimAtual,
            Status = StatusImpulsionamento.Ativo,
            ValorPago = planoAtual.Valor
        });

        await context.SaveChangesAsync();

        var service = new ImpulsionamentoService(context);

        var response = await service.ContratarPlanoAsync(usuarioId, new ContratarPlanoImpulsionamentoRequest
        {
            PlanoImpulsionamentoId = novoPlano.Id,
            CodigoReferenciaPagamento = "pag-123"
        });

        Assert.Equal(StatusImpulsionamento.PendentePagamento, response.Status);
        Assert.Equal(dataFimAtual, response.DataInicio);
        Assert.Equal(dataFimAtual.AddDays(novoPlano.QuantidadePeriodo), response.DataFim);
    }

    [Fact]
    public async Task ListarMeusImpulsionamentosAsync_DeveExpirarOAnteriorEAtivarOFuturoNaVirada()
    {
        await using var context = CriarContexto();

        var usuarioId = Guid.NewGuid();
        var usuario = new Usuario
        {
            Id = usuarioId,
            Nome = "Profissional Teste",
            Email = "profissional2@teste.local",
            Telefone = string.Empty,
            SenhaHash = "hash",
            TipoPerfil = TipoPerfil.Profissional
        };
        var profissional = new Profissional
        {
            UsuarioId = usuarioId,
            NomeExibicao = "Profissional Teste"
        };

        var plano = new PlanoImpulsionamento
        {
            Nome = "Plano",
            TipoPeriodo = TipoPeriodoImpulsionamento.Dia,
            QuantidadePeriodo = 1,
            Valor = 10m
        };

        var instanteVirada = DateTime.UtcNow.AddSeconds(-1);

        var impulsionamentoEncerrando = new ImpulsionamentoProfissional
        {
            ProfissionalId = profissional.Id,
            PlanoImpulsionamentoId = plano.Id,
            DataInicio = instanteVirada.AddDays(-1),
            DataFim = instanteVirada,
            Status = StatusImpulsionamento.Ativo,
            ValorPago = plano.Valor
        };

        var impulsionamentoAgendado = new ImpulsionamentoProfissional
        {
            ProfissionalId = profissional.Id,
            PlanoImpulsionamentoId = plano.Id,
            DataInicio = instanteVirada,
            DataFim = instanteVirada.AddDays(1),
            Status = StatusImpulsionamento.PendentePagamento,
            ValorPago = plano.Valor
        };

        context.Usuarios.Add(usuario);
        context.Profissionais.Add(profissional);
        context.PlanosImpulsionamento.Add(plano);
        context.ImpulsionamentosProfissionais.AddRange(impulsionamentoEncerrando, impulsionamentoAgendado);

        await context.SaveChangesAsync();

        var service = new ImpulsionamentoService(context);

        var response = await service.ListarMeusImpulsionamentosAsync(usuarioId);

        var atualizadoEncerrando = await context.ImpulsionamentosProfissionais
            .FirstAsync(x => x.Id == impulsionamentoEncerrando.Id);

        var atualizadoAgendado = await context.ImpulsionamentosProfissionais
            .FirstAsync(x => x.Id == impulsionamentoAgendado.Id);

        Assert.Equal(StatusImpulsionamento.Expirado, atualizadoEncerrando.Status);
        Assert.Equal(StatusImpulsionamento.Ativo, atualizadoAgendado.Status);
        Assert.Contains(response, x => x.Id == impulsionamentoEncerrando.Id && x.Status == StatusImpulsionamento.Expirado);
        Assert.Contains(response, x => x.Id == impulsionamentoAgendado.Id && x.Status == StatusImpulsionamento.Ativo);
    }

    private static AppDbContext CriarContexto()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }
}
