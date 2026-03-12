using MeAjudaAi.Domain.Common;

namespace MeAjudaAi.Domain.Entities;

public class Estado : EntityBase
{
    public string Nome { get; set; } = string.Empty;
    public string UF { get; set; } = string.Empty;
    public string CodigoIbge { get; set; } = string.Empty;

    public ICollection<Cidade> Cidades { get; set; } = new List<Cidade>();
}