using MeAjudaAi.Domain.Common;
using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Domain.Entities;

public class PlanoImpulsionamento : EntityBase
{
    public string Nome { get; set; } = string.Empty;
    public TipoPeriodoImpulsionamento TipoPeriodo { get; set; }
    public int QuantidadePeriodo { get; set; }
    public decimal Valor { get; set; }

    public ICollection<ImpulsionamentoProfissional> Impulsionamentos { get; set; } = new List<ImpulsionamentoProfissional>();
}