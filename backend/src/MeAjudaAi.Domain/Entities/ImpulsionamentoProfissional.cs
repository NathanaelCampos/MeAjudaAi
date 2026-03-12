using MeAjudaAi.Domain.Common;
using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Domain.Entities;

public class ImpulsionamentoProfissional : EntityBase
{
    public Guid ProfissionalId { get; set; }
    public Guid PlanoImpulsionamentoId { get; set; }

    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public StatusImpulsionamento Status { get; set; }
    public decimal ValorPago { get; set; }
    public string CodigoReferenciaPagamento { get; set; } = string.Empty;

    public Profissional Profissional { get; set; } = null!;
    public PlanoImpulsionamento PlanoImpulsionamento { get; set; } = null!;
}