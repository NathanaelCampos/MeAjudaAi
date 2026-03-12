using MeAjudaAi.Domain.Common;

namespace MeAjudaAi.Domain.Entities;

public class Cidade : EntityBase
{
    public Guid EstadoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string CodigoIbge { get; set; } = string.Empty;

    public Estado Estado { get; set; } = null!;

    public ICollection<Bairro> Bairros { get; set; } = new List<Bairro>();
    public ICollection<AreaAtendimento> AreasAtendimento { get; set; } = new List<AreaAtendimento>();
}