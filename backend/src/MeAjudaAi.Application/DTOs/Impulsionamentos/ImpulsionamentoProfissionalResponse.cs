using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Impulsionamentos;

public class ImpulsionamentoProfissionalResponse
{
    public Guid Id { get; set; }
    public Guid ProfissionalId { get; set; }
    public Guid PlanoImpulsionamentoId { get; set; }
    public string NomePlano { get; set; } = string.Empty;
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public StatusImpulsionamento Status { get; set; }
    public decimal ValorPago { get; set; }
    public string CodigoReferenciaPagamento { get; set; } = string.Empty;
}