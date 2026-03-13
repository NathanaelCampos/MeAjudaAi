using MeAjudaAi.Application.DTOs.Impulsionamentos;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.DTOs.Notificacoes;
using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Application.Interfaces.Notificacoes;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.Data.Sqlite;
using MeAjudaAi.Infrastructure.Services.Impulsionamentos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MeAjudaAi.Application.Interfaces.Impulsionamentos;

namespace MeAjudaAi.UnitTests.Impulsionamentos;

public class ImpulsionamentoServiceTests
{
    private static readonly INotificacaoService NotificacaoNula = new NotificacaoServiceNula();

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

        var service = new ImpulsionamentoService(
            context,
            NullLogger<ImpulsionamentoService>.Instance,
            new WebhookPagamentoMetricsService(),
            NotificacaoNula);

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
    public async Task ListarMeusImpulsionamentosAsync_DeveExpirarOAnteriorEManterOFuturoAtivoNaVirada()
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
            Status = StatusImpulsionamento.Ativo,
            ValorPago = plano.Valor
        };

        context.Usuarios.Add(usuario);
        context.Profissionais.Add(profissional);
        context.PlanosImpulsionamento.Add(plano);
        context.ImpulsionamentosProfissionais.AddRange(impulsionamentoEncerrando, impulsionamentoAgendado);

        await context.SaveChangesAsync();

        var service = new ImpulsionamentoService(
            context,
            NullLogger<ImpulsionamentoService>.Instance,
            new WebhookPagamentoMetricsService(),
            NotificacaoNula);

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

    [Fact]
    public async Task ConfirmarPagamentoAsync_DeveAtivarImpulsionamentoPendente()
    {
        await using var context = CriarContexto();

        var usuarioId = Guid.NewGuid();
        var usuario = new Usuario
        {
            Id = usuarioId,
            Nome = "Profissional Teste",
            Email = "profissional3@teste.local",
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

        var impulsionamento = new ImpulsionamentoProfissional
        {
            ProfissionalId = profissional.Id,
            PlanoImpulsionamentoId = plano.Id,
            DataInicio = DateTime.UtcNow,
            DataFim = DateTime.UtcNow.AddDays(1),
            Status = StatusImpulsionamento.PendentePagamento,
            ValorPago = plano.Valor
        };

        context.Usuarios.Add(usuario);
        context.Profissionais.Add(profissional);
        context.PlanosImpulsionamento.Add(plano);
        context.ImpulsionamentosProfissionais.Add(impulsionamento);

        await context.SaveChangesAsync();

        var service = new ImpulsionamentoService(
            context,
            NullLogger<ImpulsionamentoService>.Instance,
            new WebhookPagamentoMetricsService(),
            NotificacaoNula);

        var response = await service.ConfirmarPagamentoAsync(impulsionamento.Id);

        Assert.Equal(StatusImpulsionamento.Ativo, response.Status);

        var atualizado = await context.ImpulsionamentosProfissionais
            .FirstAsync(x => x.Id == impulsionamento.Id);

        Assert.Equal(StatusImpulsionamento.Ativo, atualizado.Status);
    }

    [Fact]
    public async Task ConfirmarPagamentoPorCodigoReferenciaAsync_DeveAtivarImpulsionamentoPendente()
    {
        await using var context = CriarContexto();

        var usuarioId = Guid.NewGuid();
        var usuario = new Usuario
        {
            Id = usuarioId,
            Nome = "Profissional Teste",
            Email = "profissional4@teste.local",
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

        var impulsionamento = new ImpulsionamentoProfissional
        {
            ProfissionalId = profissional.Id,
            PlanoImpulsionamentoId = plano.Id,
            DataInicio = DateTime.UtcNow,
            DataFim = DateTime.UtcNow.AddDays(1),
            Status = StatusImpulsionamento.PendentePagamento,
            ValorPago = plano.Valor,
            CodigoReferenciaPagamento = "pagamento-ref-001"
        };

        context.Usuarios.Add(usuario);
        context.Profissionais.Add(profissional);
        context.PlanosImpulsionamento.Add(plano);
        context.ImpulsionamentosProfissionais.Add(impulsionamento);

        await context.SaveChangesAsync();

        var service = new ImpulsionamentoService(
            context,
            NullLogger<ImpulsionamentoService>.Instance,
            new WebhookPagamentoMetricsService(),
            NotificacaoNula);

        var response = await service.ConfirmarPagamentoPorCodigoReferenciaAsync("pagamento-ref-001");

        Assert.Equal(StatusImpulsionamento.Ativo, response.Status);
        Assert.Equal("pagamento-ref-001", response.CodigoReferenciaPagamento);
    }

    [Fact]
    public async Task CancelarPorCodigoReferenciaAsync_DeveCancelarImpulsionamentoPendente()
    {
        await using var context = CriarContexto();

        var usuarioId = Guid.NewGuid();
        var usuario = new Usuario
        {
            Id = usuarioId,
            Nome = "Profissional Teste",
            Email = "profissional5@teste.local",
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

        var impulsionamento = new ImpulsionamentoProfissional
        {
            ProfissionalId = profissional.Id,
            PlanoImpulsionamentoId = plano.Id,
            DataInicio = DateTime.UtcNow,
            DataFim = DateTime.UtcNow.AddDays(1),
            Status = StatusImpulsionamento.PendentePagamento,
            ValorPago = plano.Valor,
            CodigoReferenciaPagamento = "pagamento-ref-002"
        };

        context.Usuarios.Add(usuario);
        context.Profissionais.Add(profissional);
        context.PlanosImpulsionamento.Add(plano);
        context.ImpulsionamentosProfissionais.Add(impulsionamento);

        await context.SaveChangesAsync();

        var service = new ImpulsionamentoService(
            context,
            NullLogger<ImpulsionamentoService>.Instance,
            new WebhookPagamentoMetricsService(),
            NotificacaoNula);

        var response = await service.CancelarPorCodigoReferenciaAsync("pagamento-ref-002");

        Assert.Equal(StatusImpulsionamento.Cancelado, response.Status);
        Assert.Equal("pagamento-ref-002", response.CodigoReferenciaPagamento);
    }

    [Fact]
    public async Task ProcessarWebhookPagamentoAsync_DeveSerIdempotenteParaMesmoEventoExterno()
    {
        await using var context = CriarContexto();

        var usuarioId = Guid.NewGuid();
        var usuario = new Usuario
        {
            Id = usuarioId,
            Nome = "Profissional Teste",
            Email = "profissional6@teste.local",
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

        var impulsionamento = new ImpulsionamentoProfissional
        {
            ProfissionalId = profissional.Id,
            PlanoImpulsionamentoId = plano.Id,
            DataInicio = DateTime.UtcNow,
            DataFim = DateTime.UtcNow.AddDays(1),
            Status = StatusImpulsionamento.PendentePagamento,
            ValorPago = plano.Valor,
            CodigoReferenciaPagamento = "pagamento-ref-003"
        };

        context.Usuarios.Add(usuario);
        context.Profissionais.Add(profissional);
        context.PlanosImpulsionamento.Add(plano);
        context.ImpulsionamentosProfissionais.Add(impulsionamento);

        await context.SaveChangesAsync();

        var metrics = new WebhookPagamentoMetricsService();
        var service = new ImpulsionamentoService(
            context,
            NullLogger<ImpulsionamentoService>.Instance,
            metrics,
            NotificacaoNula);
        var request = new WebhookPagamentoImpulsionamentoRequest
        {
            CodigoReferenciaPagamento = "pagamento-ref-003",
            StatusPagamento = "pago",
            EventoExternoId = "evt-unit-001"
        };

        var primeiro = await service.ProcessarWebhookPagamentoAsync(
            "padrao",
            request,
            "{\"codigoReferenciaPagamento\":\"pagamento-ref-003\"}",
            "{\"X-Webhook-Signature\":[\"abc\"]}",
            "127.0.0.1",
            "req-unit-001",
            "unit-test-agent/1.0");
        var segundo = await service.ProcessarWebhookPagamentoAsync(
            "padrao",
            request,
            "{\"codigoReferenciaPagamento\":\"pagamento-ref-003\"}",
            "{\"X-Webhook-Signature\":[\"abc\"]}",
            "127.0.0.1",
            "req-unit-001",
            "unit-test-agent/1.0");

        Assert.False(primeiro.Duplicado);
        Assert.Equal("padrao", primeiro.Provedor);
        Assert.True(segundo.Duplicado);
        Assert.NotNull(segundo.Impulsionamento);
        Assert.Equal(StatusImpulsionamento.Ativo, segundo.Impulsionamento!.Status);
        Assert.Equal(1, await context.WebhookPagamentoImpulsionamentoEventos.CountAsync());

        var evento = await context.WebhookPagamentoImpulsionamentoEventos.SingleAsync();
        Assert.Equal("{\"X-Webhook-Signature\":[\"abc\"]}", evento.HeadersJson);
        Assert.Equal("127.0.0.1", evento.IpOrigem);
        Assert.Equal("req-unit-001", evento.RequestId);
        Assert.Equal("unit-test-agent/1.0", evento.UserAgent);

        var metricas = metrics.ObterSnapshot();
        Assert.Contains(metricas.Itens, x => x.Resultado == "recebido" && x.Provedor == "padrao" && x.StatusRecebido == "pago" && x.Quantidade == 2);
        Assert.Contains(metricas.Itens, x => x.Resultado == "processado" && x.Provedor == "padrao" && x.StatusRecebido == "pago" && x.Quantidade == 1);
        Assert.Contains(metricas.Itens, x => x.Resultado == "duplicado" && x.Provedor == "padrao" && x.StatusRecebido == "pago" && x.Quantidade == 1);
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

    private sealed class NotificacaoServiceNula : INotificacaoService
    {
        public Task CriarAsync(Guid usuarioId, TipoNotificacao tipo, string titulo, string mensagem, Guid? referenciaId = null, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IReadOnlyList<NotificacaoResponse>> ListarMinhasAsync(Guid usuarioId, bool somenteNaoLidas = false, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<NotificacaoResponse>>(Array.Empty<NotificacaoResponse>());

        public Task<IReadOnlyList<PreferenciaNotificacaoResponse>> ListarPreferenciasAsync(Guid usuarioId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<PreferenciaNotificacaoResponse>>(Array.Empty<PreferenciaNotificacaoResponse>());

        public Task<IReadOnlyList<PreferenciaNotificacaoResponse>> AtualizarPreferenciasAsync(Guid usuarioId, IReadOnlyList<PreferenciaNotificacaoItemRequest> preferencias, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<PreferenciaNotificacaoResponse>>(Array.Empty<PreferenciaNotificacaoResponse>());

        public Task<PaginacaoResponse<NotificacaoAdminResponse>> ListarNotificacoesAsync(BuscarNotificacoesRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new PaginacaoResponse<NotificacaoAdminResponse>());

        public Task<PaginacaoResponse<NotificacaoAdminResponse>> ListarNotificacoesArquivadasAsync(BuscarNotificacoesRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new PaginacaoResponse<NotificacaoAdminResponse>());

        public Task<string> ExportarNotificacoesCsvAsync(ExportarNotificacoesRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(string.Empty);

        public Task<string> ExportarNotificacoesArquivadasCsvAsync(ExportarNotificacoesRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(string.Empty);

        public Task<NotificacaoAdminResponse?> ObterNotificacaoPorIdAsync(Guid notificacaoId, CancellationToken cancellationToken = default)
            => Task.FromResult<NotificacaoAdminResponse?>(null);

        public Task<NotificacaoAdminResponse?> ObterNotificacaoArquivadaPorIdAsync(Guid notificacaoId, CancellationToken cancellationToken = default)
            => Task.FromResult<NotificacaoAdminResponse?>(null);

        public Task<int> MarcarNotificacoesComoLidasEmLoteAsync(MarcarNotificacoesComoLidasEmLoteRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<int> ArquivarNotificacoesEmLoteAsync(ArquivarNotificacoesEmLoteRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<int> RestaurarNotificacoesEmLoteAsync(ArquivarNotificacoesEmLoteRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<int> ExcluirNotificacoesArquivadasEmLoteAsync(ArquivarNotificacoesEmLoteRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<PreviewArquivamentoNotificacoesResponse> PreviewArquivamentoNotificacoesAsync(ArquivarNotificacoesEmLoteRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new PreviewArquivamentoNotificacoesResponse());

        public Task<PreviewArquivamentoNotificacoesResponse> PreviewRestauracaoNotificacoesAsync(ArquivarNotificacoesEmLoteRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new PreviewArquivamentoNotificacoesResponse());

        public Task<PreviewArquivamentoNotificacoesResponse> PreviewExclusaoNotificacoesArquivadasAsync(ArquivarNotificacoesEmLoteRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new PreviewArquivamentoNotificacoesResponse());

        public Task<PreviewExclusaoNotificacoesAntigasResponse> ObterAntigasExclusaoNotificacoesArquivadasAsync(ArquivarNotificacoesEmLoteRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new PreviewExclusaoNotificacoesAntigasResponse());

        public Task<NotificacaoResumoOperacionalResponse> ObterResumoOperacionalNotificacoesAsync(Guid? usuarioId = null, TipoNotificacao? tipoNotificacao = null, DateTime? dataCriacaoInicial = null, DateTime? dataCriacaoFinal = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new NotificacaoResumoOperacionalResponse());

        public Task<NotificacaoResumoOperacionalResponse> ObterResumoOperacionalNotificacoesArquivadasAsync(Guid? usuarioId = null, TipoNotificacao? tipoNotificacao = null, DateTime? dataCriacaoInicial = null, DateTime? dataCriacaoFinal = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new NotificacaoResumoOperacionalResponse());

        public Task<NotificacaoResumoOperacionalResponse> ObterResumoOperacionalExclusaoNotificacoesArquivadasAsync(Guid? usuarioId = null, TipoNotificacao? tipoNotificacao = null, DateTime? dataCriacaoInicial = null, DateTime? dataCriacaoFinal = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new NotificacaoResumoOperacionalResponse());

        public Task<NotificacaoArquivadaResumoIdadeResponse> ObterResumoIdadeExclusaoNotificacoesArquivadasAsync(Guid? usuarioId = null, TipoNotificacao? tipoNotificacao = null, DateTime? dataCriacaoInicial = null, DateTime? dataCriacaoFinal = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new NotificacaoArquivadaResumoIdadeResponse());

        public Task<NotificacaoArquivadaResumoTiposResponse> ObterResumoTiposExclusaoNotificacoesArquivadasAsync(Guid? usuarioId = null, TipoNotificacao? tipoNotificacao = null, DateTime? dataCriacaoInicial = null, DateTime? dataCriacaoFinal = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new NotificacaoArquivadaResumoTiposResponse());

        public Task<NotificacaoArquivadaResumoUsuariosResponse> ObterResumoUsuariosExclusaoNotificacoesArquivadasAsync(Guid? usuarioId = null, TipoNotificacao? tipoNotificacao = null, DateTime? dataCriacaoInicial = null, DateTime? dataCriacaoFinal = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new NotificacaoArquivadaResumoUsuariosResponse());

        public Task<NotificacaoArquivadaMetricasSerieResponse> ObterSerieExclusaoNotificacoesArquivadasAsync(Guid? usuarioId = null, TipoNotificacao? tipoNotificacao = null, DateTime? dataCriacaoInicial = null, DateTime? dataCriacaoFinal = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new NotificacaoArquivadaMetricasSerieResponse());

        public Task<NotificacaoArquivadaResumoLeituraResponse> ObterResumoLeituraExclusaoNotificacoesArquivadasAsync(Guid? usuarioId = null, TipoNotificacao? tipoNotificacao = null, DateTime? dataCriacaoInicial = null, DateTime? dataCriacaoFinal = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new NotificacaoArquivadaResumoLeituraResponse());

        public Task<NotificacaoArquivadaResumoLimitesResponse> ObterResumoLimitesExclusaoNotificacoesArquivadasAsync(Guid? usuarioId = null, TipoNotificacao? tipoNotificacao = null, DateTime? dataCriacaoInicial = null, DateTime? dataCriacaoFinal = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new NotificacaoArquivadaResumoLimitesResponse());

        public Task<NotificacaoArquivadaExclusaoDashboardResponse> ObterDashboardExclusaoNotificacoesArquivadasAsync(Guid? usuarioId = null, TipoNotificacao? tipoNotificacao = null, DateTime? dataCriacaoInicial = null, DateTime? dataCriacaoFinal = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new NotificacaoArquivadaExclusaoDashboardResponse());

        public Task<NotificacaoUsuarioDashboardResponse> ObterDashboardNotificacoesPorUsuarioAsync(Guid usuarioId, TipoNotificacao? tipoNotificacao = null, DateTime? dataCriacaoInicial = null, DateTime? dataCriacaoFinal = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new NotificacaoUsuarioDashboardResponse());

        public Task<NotificacaoUsuarioDashboardResponse> ObterDashboardNotificacoesArquivadasPorUsuarioAsync(Guid usuarioId, TipoNotificacao? tipoNotificacao = null, DateTime? dataCriacaoInicial = null, DateTime? dataCriacaoFinal = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new NotificacaoUsuarioDashboardResponse());

        public Task<NotificacaoUsuarioDashboardResponse> ObterDashboardExclusaoNotificacoesArquivadasPorUsuarioAsync(Guid usuarioId, TipoNotificacao? tipoNotificacao = null, DateTime? dataCriacaoInicial = null, DateTime? dataCriacaoFinal = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new NotificacaoUsuarioDashboardResponse());

        public Task<PaginacaoResponse<EmailNotificacaoOutboxResponse>> ListarEmailsOutboxAsync(BuscarEmailsOutboxRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new PaginacaoResponse<EmailNotificacaoOutboxResponse>());

        public Task<string> ExportarEmailsOutboxCsvAsync(ExportarEmailsOutboxRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(string.Empty);

        public Task<EmailNotificacaoOutboxResponse?> ObterEmailOutboxPorIdAsync(Guid emailId, CancellationToken cancellationToken = default)
            => Task.FromResult<EmailNotificacaoOutboxResponse?>(null);

        public Task<EmailNotificacaoOutboxResponse?> CancelarEmailOutboxAsync(Guid emailId, CancellationToken cancellationToken = default)
            => Task.FromResult<EmailNotificacaoOutboxResponse?>(null);

        public Task<EmailNotificacaoOutboxResponse?> ReabrirEmailOutboxAsync(Guid emailId, CancellationToken cancellationToken = default)
            => Task.FromResult<EmailNotificacaoOutboxResponse?>(null);

        public Task<int> CancelarEmailsOutboxEmLoteAsync(AtualizarEmailsOutboxEmLoteRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<int> ReabrirEmailsOutboxEmLoteAsync(AtualizarEmailsOutboxEmLoteRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<int> ReprocessarEmailsOutboxEmLoteAsync(AtualizarEmailsOutboxEmLoteRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<int> ReprocessarEmailsOutboxAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<EmailNotificacaoMetricasResponse> ObterMetricasEmailsOutboxAsync(BuscarMetricasEmailsOutboxRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new EmailNotificacaoMetricasResponse());

        public Task<EmailNotificacaoResumoOperacionalResponse> ObterResumoOperacionalEmailsOutboxAsync(BuscarMetricasEmailsOutboxRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new EmailNotificacaoResumoOperacionalResponse());

        public Task<EmailNotificacaoMetricasSerieResponse> ObterMetricasSerieEmailsOutboxAsync(BuscarMetricasEmailsOutboxRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new EmailNotificacaoMetricasSerieResponse());

        public Task<EmailNotificacaoDestinatariosMetricasResponse> ObterMetricasDestinatariosEmailsOutboxAsync(BuscarMetricasEmailsOutboxRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new EmailNotificacaoDestinatariosMetricasResponse());

        public Task<EmailNotificacaoTiposMetricasResponse> ObterMetricasTiposEmailsOutboxAsync(BuscarMetricasEmailsOutboxRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new EmailNotificacaoTiposMetricasResponse());

        public Task<EmailNotificacaoDashboardResponse> ObterDashboardEmailsOutboxAsync(BuscarMetricasEmailsOutboxRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new EmailNotificacaoDashboardResponse());

        public Task<EmailNotificacaoUsuarioDashboardResponse> ObterDashboardEmailsOutboxPorUsuarioAsync(Guid usuarioId, BuscarMetricasEmailsOutboxRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new EmailNotificacaoUsuarioDashboardResponse());

        public Task<QuantidadeNotificacoesNaoLidasResponse> ObterQuantidadeNaoLidasAsync(Guid usuarioId, CancellationToken cancellationToken = default)
            => Task.FromResult(new QuantidadeNotificacoesNaoLidasResponse());

        public Task<NotificacaoResponse?> MarcarComoLidaAsync(Guid usuarioId, Guid notificacaoId, CancellationToken cancellationToken = default)
            => Task.FromResult<NotificacaoResponse?>(null);

        public Task<int> MarcarTodasComoLidasAsync(Guid usuarioId, CancellationToken cancellationToken = default)
            => Task.FromResult(0);
    }
}
