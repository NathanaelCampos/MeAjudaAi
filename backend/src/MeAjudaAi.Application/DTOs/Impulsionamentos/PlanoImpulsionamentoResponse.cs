using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Impulsionamentos;

public class PlanoImpulsionamentoResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public TipoPeriodoImpulsionamento TipoPeriodo { get; set; }
    public int QuantidadePeriodo { get; set; }
    public decimal Valor { get; set; }
}