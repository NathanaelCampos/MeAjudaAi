using System.Data;
using System.Text.Json;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.DTOs.Impulsionamentos;
using MeAjudaAi.Application.Interfaces.Impulsionamentos;
using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace MeAjudaAi.Infrastructure.Services.Impulsionamentos;

public class ImpulsionamentoService : IImpulsionamentoService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ImpulsionamentoService> _logger;
    private readonly IWebhookPagamentoMetricsService _metricsService;

    public ImpulsionamentoService(
        AppDbContext context,
        ILogger<ImpulsionamentoService> logger,
        IWebhookPagamentoMetricsService metricsService)
    {
        _context = context;
        _logger = logger;
        _metricsService = metricsService;
    }

    public async Task<IReadOnlyList<PlanoImpulsionamentoResponse>> ListarPlanosAsync(
        CancellationToken cancellationToken = default)
    {
        var planos = await _context.PlanosImpulsionamento
            .AsNoTracking()
            .Where(x => x.Ativo)
            .Select(x => new PlanoImpulsionamentoResponse
            {
                Id = x.Id,
                Nome = x.Nome,
                TipoPeriodo = x.TipoPeriodo,
                QuantidadePeriodo = x.QuantidadePeriodo,
                Valor = x.Valor
            })
            .ToListAsync(cancellationToken);

        return planos
            .OrderBy(x => x.Valor)
            .ThenBy(x => x.Nome)
            .ToList();
    }

    public async Task<PaginacaoResponse<WebhookPagamentoImpulsionamentoEventoResponse>> ListarWebhooksAsync(
        BuscarWebhookPagamentosRequest request,
        CancellationToken cancellationToken = default)
    {
        var pagina = request.Pagina <= 0 ? 1 : request.Pagina;
        var tamanhoPagina = request.TamanhoPagina <= 0 ? 20 : Math.Min(request.TamanhoPagina, 100);

        var query = _context.WebhookPagamentoImpulsionamentoEventos
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.EventoExternoId))
        {
            var eventoExternoId = request.EventoExternoId.Trim();
            query = query.Where(x => x.EventoExternoId == eventoExternoId);
        }

        if (!string.IsNullOrWhiteSpace(request.CodigoReferenciaPagamento))
        {
            var codigoReferencia = request.CodigoReferenciaPagamento.Trim();
            query = query.Where(x => x.CodigoReferenciaPagamento == codigoReferencia);
        }

        if (!string.IsNullOrWhiteSpace(request.Provedor))
        {
            var provedor = request.Provedor.Trim().ToLowerInvariant();
            query = query.Where(x => x.Provedor == provedor);
        }

        var totalRegistros = await query.CountAsync(cancellationToken);

        var itens = await query
            .OrderByDescending(x => x.DataCriacao)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(x => new WebhookPagamentoImpulsionamentoEventoResponse
            {
                Id = x.Id,
                Provedor = x.Provedor,
                EventoExternoId = x.EventoExternoId,
                CodigoReferenciaPagamento = x.CodigoReferenciaPagamento,
                StatusPagamento = x.StatusPagamento,
                ProcessadoComSucesso = x.ProcessadoComSucesso,
                MensagemResultado = x.MensagemResultado,
                IpOrigem = x.IpOrigem,
                RequestId = x.RequestId,
                UserAgent = x.UserAgent,
                ImpulsionamentoProfissionalId = x.ImpulsionamentoProfissionalId,
                StatusImpulsionamentoResultado = x.StatusImpulsionamentoResultado.HasValue
                    ? (int)x.StatusImpulsionamentoResultado.Value
                    : null,
                DataCriacao = x.DataCriacao
            })
            .ToListAsync(cancellationToken);

        return new PaginacaoResponse<WebhookPagamentoImpulsionamentoEventoResponse>
        {
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalRegistros = totalRegistros,
            TotalPaginas = totalRegistros == 0 ? 0 : (int)Math.Ceiling(totalRegistros / (double)tamanhoPagina),
            Itens = itens
        };
    }

    public async Task<ImpulsionamentoProfissionalResponse> ContratarPlanoAsync(
    Guid usuarioId,
    ContratarPlanoImpulsionamentoRequest request,
    CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            await AtualizarImpulsionamentosExpiradosAsync(cancellationToken);

            var profissional = await _context.Profissionais
                .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId, cancellationToken);

            if (profissional is null)
                throw new InvalidOperationException("Profissional não encontrado para o usuário autenticado.");

            var plano = await _context.PlanosImpulsionamento
                .FirstOrDefaultAsync(x => x.Id == request.PlanoImpulsionamentoId && x.Ativo, cancellationToken);

            if (plano is null)
                throw new InvalidOperationException("Plano de impulsionamento não encontrado.");

            var agora = DateTime.UtcNow;

            var ultimoImpulsionamentoVigente = await _context.ImpulsionamentosProfissionais
                .Where(x =>
                    x.ProfissionalId == profissional.Id &&
                    (x.Status == StatusImpulsionamento.Ativo || x.Status == StatusImpulsionamento.PendentePagamento) &&
                    x.DataFim > agora)
                .OrderByDescending(x => x.DataFim)
                .FirstOrDefaultAsync(cancellationToken);

            var dataInicio = ultimoImpulsionamentoVigente is not null && ultimoImpulsionamentoVigente.DataFim > agora
                ? ultimoImpulsionamentoVigente.DataFim
                : agora;

            var dataFim = plano.TipoPeriodo switch
            {
                TipoPeriodoImpulsionamento.Hora => dataInicio.AddHours(plano.QuantidadePeriodo),
                TipoPeriodoImpulsionamento.Dia => dataInicio.AddDays(plano.QuantidadePeriodo),
                TipoPeriodoImpulsionamento.Semana => dataInicio.AddDays(plano.QuantidadePeriodo * 7),
                TipoPeriodoImpulsionamento.Mes => dataInicio.AddMonths(plano.QuantidadePeriodo),
                _ => dataInicio.AddDays(plano.QuantidadePeriodo)
            };

            var impulsionamento = new ImpulsionamentoProfissional
            {
                ProfissionalId = profissional.Id,
                PlanoImpulsionamentoId = plano.Id,
                DataInicio = dataInicio,
                DataFim = dataFim,
                Status = StatusImpulsionamento.PendentePagamento,
                ValorPago = plano.Valor,
                CodigoReferenciaPagamento = request.CodigoReferenciaPagamento?.Trim() ?? string.Empty
            };

            _context.ImpulsionamentosProfissionais.Add(impulsionamento);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return MapearResponse(impulsionamento, plano.Nome);
        }
        catch (DbUpdateException ex) when (EhConflitoImpulsionamento(ex))
        {
            throw new InvalidOperationException(
                "Já existe um impulsionamento ativo ou agendado em conflito para este profissional.",
                ex);
        }
        catch (PostgresException ex) when (EhConflitoImpulsionamento(ex))
        {
            throw new InvalidOperationException(
                "Já existe um impulsionamento ativo ou agendado em conflito para este profissional.",
                ex);
        }
    }

    public async Task<ImpulsionamentoProfissionalResponse> ConfirmarPagamentoAsync(
        Guid impulsionamentoId,
        CancellationToken cancellationToken = default)
    {
        await AtualizarImpulsionamentosExpiradosAsync(cancellationToken);

        var impulsionamento = await _context.ImpulsionamentosProfissionais
            .Include(x => x.PlanoImpulsionamento)
            .FirstOrDefaultAsync(x => x.Id == impulsionamentoId, cancellationToken);

        if (impulsionamento is null)
            throw new InvalidOperationException("Impulsionamento não encontrado.");

        if (impulsionamento.Status == StatusImpulsionamento.Cancelado ||
            impulsionamento.Status == StatusImpulsionamento.Encerrado ||
            impulsionamento.Status == StatusImpulsionamento.Expirado)
        {
            throw new InvalidOperationException("Não é possível confirmar pagamento para um impulsionamento encerrado.");
        }

        if (impulsionamento.Status == StatusImpulsionamento.Ativo)
            return MapearResponse(impulsionamento, impulsionamento.PlanoImpulsionamento.Nome);

        return await AtivarImpulsionamentoAsync(impulsionamento, cancellationToken);
    }

    public async Task<ImpulsionamentoProfissionalResponse> ConfirmarPagamentoPorCodigoReferenciaAsync(
        string codigoReferenciaPagamento,
        CancellationToken cancellationToken = default)
    {
        await AtualizarImpulsionamentosExpiradosAsync(cancellationToken);

        var codigoNormalizado = codigoReferenciaPagamento.Trim();

        var impulsionamentos = await _context.ImpulsionamentosProfissionais
            .Include(x => x.PlanoImpulsionamento)
            .Where(x => x.CodigoReferenciaPagamento == codigoNormalizado)
            .ToListAsync(cancellationToken);

        if (impulsionamentos.Count == 0)
            throw new InvalidOperationException("Impulsionamento não encontrado para o código de referência informado.");

        if (impulsionamentos.Count > 1)
            throw new InvalidOperationException("Há mais de um impulsionamento com o mesmo código de referência.");

        var impulsionamento = impulsionamentos[0];

        if (impulsionamento.Status == StatusImpulsionamento.Cancelado ||
            impulsionamento.Status == StatusImpulsionamento.Encerrado ||
            impulsionamento.Status == StatusImpulsionamento.Expirado)
        {
            throw new InvalidOperationException("Não é possível confirmar pagamento para um impulsionamento encerrado.");
        }

        if (impulsionamento.Status == StatusImpulsionamento.Ativo)
            return MapearResponse(impulsionamento, impulsionamento.PlanoImpulsionamento.Nome);

        return await AtivarImpulsionamentoAsync(impulsionamento, cancellationToken);
    }

    public async Task<WebhookPagamentoImpulsionamentoResponse> ProcessarWebhookPagamentoAsync(
        string provedor,
        WebhookPagamentoImpulsionamentoRequest request,
        string payloadJson,
        string headersJson,
        string ipOrigem,
        string requestId,
        string userAgent,
        CancellationToken cancellationToken = default)
    {
        var provedorNormalizado = string.IsNullOrWhiteSpace(provedor) ? "padrao" : provedor.Trim().ToLowerInvariant();
        var eventoExternoId = request.EventoExternoId?.Trim() ?? string.Empty;
        var codigoReferenciaPagamento = request.CodigoReferenciaPagamento?.Trim() ?? string.Empty;
        var statusRecebido = request.StatusPagamento?.Trim().ToLowerInvariant() ?? string.Empty;

        _logger.LogInformation(
            "Webhook pagamento recebido. Provedor={Provedor} EventoExternoId={EventoExternoId} CodigoReferenciaPagamento={CodigoReferenciaPagamento} StatusRecebido={StatusRecebido} RequestId={RequestId} IpOrigem={IpOrigem}",
            provedorNormalizado,
            eventoExternoId,
            codigoReferenciaPagamento,
            statusRecebido,
            requestId,
            ipOrigem);
        _metricsService.RegistrarRecebido(provedorNormalizado, statusRecebido);

        var eventoExistente = await _context.WebhookPagamentoImpulsionamentoEventos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.EventoExternoId == eventoExternoId, cancellationToken);

        if (eventoExistente is not null)
        {
            _logger.LogInformation(
                "Webhook pagamento duplicado ignorado. Provedor={Provedor} EventoExternoId={EventoExternoId} CodigoReferenciaPagamento={CodigoReferenciaPagamento} RequestId={RequestId}",
                provedorNormalizado,
                eventoExternoId,
                codigoReferenciaPagamento,
                requestId);
            _metricsService.RegistrarDuplicado(provedorNormalizado, statusRecebido);
            return await ObterRespostaWebhookExistenteAsync(eventoExistente, cancellationToken);
        }

        try
        {
            var response = statusRecebido switch
            {
                "pago" => await ConfirmarPagamentoPorCodigoReferenciaAsync(codigoReferenciaPagamento, cancellationToken),
                "cancelado" or "recusado" or "estornado" or "expirado" =>
                    await CancelarPorCodigoReferenciaAsync(codigoReferenciaPagamento, cancellationToken),
                _ => throw new InvalidOperationException("Status do pagamento inválido.")
            };

            _context.WebhookPagamentoImpulsionamentoEventos.Add(new WebhookPagamentoImpulsionamentoEvento
            {
                Provedor = provedorNormalizado,
                EventoExternoId = eventoExternoId,
                CodigoReferenciaPagamento = codigoReferenciaPagamento,
                StatusPagamento = statusRecebido,
                PayloadJson = payloadJson,
                HeadersJson = headersJson,
                IpOrigem = ipOrigem,
                RequestId = requestId,
                UserAgent = userAgent,
                ProcessadoComSucesso = true,
                MensagemResultado = "Webhook processado.",
                ImpulsionamentoProfissionalId = response.Id,
                StatusImpulsionamentoResultado = response.Status
            });

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                if (await EventoJaPersistidoAsync(eventoExternoId, ex, cancellationToken))
                {
                    var eventoDuplicado = await _context.WebhookPagamentoImpulsionamentoEventos
                        .AsNoTracking()
                        .FirstAsync(x => x.EventoExternoId == eventoExternoId, cancellationToken);

                    return await ObterRespostaWebhookExistenteAsync(eventoDuplicado, cancellationToken);
                }

                throw;
            }

            _metricsService.RegistrarProcessado(provedorNormalizado, statusRecebido);

            return new WebhookPagamentoImpulsionamentoResponse
            {
                Provedor = provedorNormalizado,
                Mensagem = "Webhook processado.",
                EventoExternoId = eventoExternoId,
                StatusRecebido = statusRecebido,
                Duplicado = false,
                Impulsionamento = response
            };
        }
        catch (InvalidOperationException ex)
        {
            _metricsService.RegistrarRejeitado(provedorNormalizado, statusRecebido);
            _logger.LogWarning(
                ex,
                "Webhook pagamento rejeitado por regra de negócio. Provedor={Provedor} EventoExternoId={EventoExternoId} CodigoReferenciaPagamento={CodigoReferenciaPagamento} StatusRecebido={StatusRecebido} RequestId={RequestId}",
                provedorNormalizado,
                eventoExternoId,
                codigoReferenciaPagamento,
                statusRecebido,
                requestId);

            _context.WebhookPagamentoImpulsionamentoEventos.Add(new WebhookPagamentoImpulsionamentoEvento
            {
                Provedor = provedorNormalizado,
                EventoExternoId = eventoExternoId,
                CodigoReferenciaPagamento = codigoReferenciaPagamento,
                StatusPagamento = statusRecebido,
                PayloadJson = payloadJson,
                HeadersJson = headersJson,
                IpOrigem = ipOrigem,
                RequestId = requestId,
                UserAgent = userAgent,
                ProcessadoComSucesso = false,
                MensagemResultado = ex.Message
            });

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException dbEx)
            {
                if (await EventoJaPersistidoAsync(eventoExternoId, dbEx, cancellationToken))
                {
                    var eventoDuplicado = await _context.WebhookPagamentoImpulsionamentoEventos
                        .AsNoTracking()
                        .FirstAsync(x => x.EventoExternoId == eventoExternoId, cancellationToken);

                    return await ObterRespostaWebhookExistenteAsync(eventoDuplicado, cancellationToken);
                }

                throw;
            }

            throw;
        }
        catch (Exception ex)
        {
            _metricsService.RegistrarErro(provedorNormalizado, statusRecebido);
            _logger.LogError(
                ex,
                "Webhook pagamento falhou com erro inesperado. Provedor={Provedor} EventoExternoId={EventoExternoId} CodigoReferenciaPagamento={CodigoReferenciaPagamento} StatusRecebido={StatusRecebido} RequestId={RequestId}",
                provedorNormalizado,
                eventoExternoId,
                codigoReferenciaPagamento,
                statusRecebido,
                requestId);

            throw;
        }
    }

    public async Task<ImpulsionamentoProfissionalResponse> CancelarPorCodigoReferenciaAsync(
        string codigoReferenciaPagamento,
        CancellationToken cancellationToken = default)
    {
        await AtualizarImpulsionamentosExpiradosAsync(cancellationToken);

        var codigoNormalizado = codigoReferenciaPagamento.Trim();

        var impulsionamentos = await _context.ImpulsionamentosProfissionais
            .Include(x => x.PlanoImpulsionamento)
            .Where(x => x.CodigoReferenciaPagamento == codigoNormalizado)
            .ToListAsync(cancellationToken);

        if (impulsionamentos.Count == 0)
            throw new InvalidOperationException("Impulsionamento não encontrado para o código de referência informado.");

        if (impulsionamentos.Count > 1)
            throw new InvalidOperationException("Há mais de um impulsionamento com o mesmo código de referência.");

        var impulsionamento = impulsionamentos[0];

        if (impulsionamento.Status == StatusImpulsionamento.Expirado ||
            impulsionamento.Status == StatusImpulsionamento.Encerrado)
        {
            throw new InvalidOperationException("Não é possível cancelar um impulsionamento encerrado.");
        }

        if (impulsionamento.Status == StatusImpulsionamento.Cancelado)
            return MapearResponse(impulsionamento, impulsionamento.PlanoImpulsionamento.Nome);

        impulsionamento.Status = StatusImpulsionamento.Cancelado;
        impulsionamento.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapearResponse(impulsionamento, impulsionamento.PlanoImpulsionamento.Nome);
    }

    public async Task<IReadOnlyList<ImpulsionamentoProfissionalResponse>> ListarMeusImpulsionamentosAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        await AtualizarImpulsionamentosExpiradosAsync(cancellationToken);

        var profissional = await _context.Profissionais
            .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId, cancellationToken);

        if (profissional is null)
            throw new InvalidOperationException("Profissional não encontrado para o usuário autenticado.");

        return await _context.ImpulsionamentosProfissionais
            .AsNoTracking()
            .Where(x => x.ProfissionalId == profissional.Id)
            .OrderByDescending(x => x.DataInicio)
            .Select(x => new ImpulsionamentoProfissionalResponse
            {
                Id = x.Id,
                ProfissionalId = x.ProfissionalId,
                PlanoImpulsionamentoId = x.PlanoImpulsionamentoId,
                NomePlano = x.PlanoImpulsionamento.Nome,
                DataInicio = x.DataInicio,
                DataFim = x.DataFim,
                Status = x.Status,
                ValorPago = x.ValorPago,
                CodigoReferenciaPagamento = x.CodigoReferenciaPagamento
            })
            .ToListAsync(cancellationToken);
    }

    private async Task AtualizarImpulsionamentosExpiradosAsync(
    CancellationToken cancellationToken = default)
    {
        var agora = DateTime.UtcNow;

        var impulsionamentos = await _context.ImpulsionamentosProfissionais
            .Where(x =>
                x.Status == StatusImpulsionamento.Ativo ||
                x.Status == StatusImpulsionamento.PendentePagamento)
            .ToListAsync(cancellationToken);

        if (impulsionamentos.Count == 0)
            return;

        var houveAlteracao = false;

        foreach (var item in impulsionamentos)
        {
            if (item.Status == StatusImpulsionamento.PendentePagamento && item.DataFim <= agora)
            {
                item.Status = StatusImpulsionamento.Expirado;
                item.DataAtualizacao = agora;
                houveAlteracao = true;
            }

            if (item.Status == StatusImpulsionamento.Ativo && item.DataFim <= agora)
            {
                item.Status = StatusImpulsionamento.Expirado;
                item.DataAtualizacao = agora;
                houveAlteracao = true;
            }
        }

        if (houveAlteracao)
            await _context.SaveChangesAsync(cancellationToken);
    }

    private static bool EhConflitoImpulsionamento(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException postgresException &&
               EhConflitoImpulsionamento(postgresException);
    }

    private static bool EhConflitoImpulsionamento(PostgresException exception)
    {
        return exception.SqlState is PostgresErrorCodes.ExclusionViolation or PostgresErrorCodes.SerializationFailure;
    }

    private async Task<WebhookPagamentoImpulsionamentoResponse> ObterRespostaWebhookExistenteAsync(
        WebhookPagamentoImpulsionamentoEvento evento,
        CancellationToken cancellationToken)
    {
        if (!evento.ProcessadoComSucesso)
            throw new InvalidOperationException(evento.MensagemResultado);

        ImpulsionamentoProfissionalResponse? impulsionamento = null;

        if (evento.ImpulsionamentoProfissionalId.HasValue)
        {
            var entidade = await _context.ImpulsionamentosProfissionais
                .AsNoTracking()
                .Include(x => x.PlanoImpulsionamento)
                .FirstOrDefaultAsync(x => x.Id == evento.ImpulsionamentoProfissionalId.Value, cancellationToken);

            if (entidade is not null)
                impulsionamento = MapearResponse(entidade, entidade.PlanoImpulsionamento.Nome);
        }

        return new WebhookPagamentoImpulsionamentoResponse
        {
            Provedor = evento.Provedor,
            Mensagem = "Webhook já processado.",
            EventoExternoId = evento.EventoExternoId,
            StatusRecebido = evento.StatusPagamento,
            Duplicado = true,
            Impulsionamento = impulsionamento
        };
    }

    private async Task<bool> EventoJaPersistidoAsync(
        string eventoExternoId,
        DbUpdateException exception,
        CancellationToken cancellationToken)
    {
        var conflitoUnico =
            exception.InnerException?.GetType().Name == "SqliteException" ||
            exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

        if (!conflitoUnico)
            return false;

        _context.ChangeTracker.Clear();

        return await _context.WebhookPagamentoImpulsionamentoEventos
            .AsNoTracking()
            .AnyAsync(x => x.EventoExternoId == eventoExternoId, cancellationToken);
    }

    private async Task<ImpulsionamentoProfissionalResponse> AtivarImpulsionamentoAsync(
        ImpulsionamentoProfissional impulsionamento,
        CancellationToken cancellationToken)
    {
        impulsionamento.Status = StatusImpulsionamento.Ativo;
        impulsionamento.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapearResponse(impulsionamento, impulsionamento.PlanoImpulsionamento.Nome);
    }

    private static ImpulsionamentoProfissionalResponse MapearResponse(
        ImpulsionamentoProfissional impulsionamento,
        string nomePlano)
    {
        return new ImpulsionamentoProfissionalResponse
        {
            Id = impulsionamento.Id,
            ProfissionalId = impulsionamento.ProfissionalId,
            PlanoImpulsionamentoId = impulsionamento.PlanoImpulsionamentoId,
            NomePlano = nomePlano,
            DataInicio = impulsionamento.DataInicio,
            DataFim = impulsionamento.DataFim,
            Status = impulsionamento.Status,
            ValorPago = impulsionamento.ValorPago,
            CodigoReferenciaPagamento = impulsionamento.CodigoReferenciaPagamento
        };
    }
}
