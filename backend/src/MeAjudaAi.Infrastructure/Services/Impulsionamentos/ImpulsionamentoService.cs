using System.Data;
using MeAjudaAi.Application.DTOs.Impulsionamentos;
using MeAjudaAi.Application.Interfaces.Impulsionamentos;
using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MeAjudaAi.Infrastructure.Services.Impulsionamentos;

public class ImpulsionamentoService : IImpulsionamentoService
{
    private readonly AppDbContext _context;

    public ImpulsionamentoService(AppDbContext context)
    {
        _context = context;
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

            var statusInicial = dataInicio <= agora
                ? StatusImpulsionamento.Ativo
                : StatusImpulsionamento.PendentePagamento;

            var impulsionamento = new ImpulsionamentoProfissional
            {
                ProfissionalId = profissional.Id,
                PlanoImpulsionamentoId = plano.Id,
                DataInicio = dataInicio,
                DataFim = dataFim,
                Status = statusInicial,
                ValorPago = plano.Valor,
                CodigoReferenciaPagamento = request.CodigoReferenciaPagamento?.Trim() ?? string.Empty
            };

            _context.ImpulsionamentosProfissionais.Add(impulsionamento);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new ImpulsionamentoProfissionalResponse
            {
                Id = impulsionamento.Id,
                ProfissionalId = impulsionamento.ProfissionalId,
                PlanoImpulsionamentoId = impulsionamento.PlanoImpulsionamentoId,
                NomePlano = plano.Nome,
                DataInicio = impulsionamento.DataInicio,
                DataFim = impulsionamento.DataFim,
                Status = impulsionamento.Status,
                ValorPago = impulsionamento.ValorPago,
                CodigoReferenciaPagamento = impulsionamento.CodigoReferenciaPagamento
            };
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
            if (item.Status == StatusImpulsionamento.PendentePagamento &&
                item.DataInicio <= agora &&
                item.DataFim > agora)
            {
                item.Status = StatusImpulsionamento.Ativo;
                item.DataAtualizacao = agora;
                houveAlteracao = true;
                continue;
            }

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
}
