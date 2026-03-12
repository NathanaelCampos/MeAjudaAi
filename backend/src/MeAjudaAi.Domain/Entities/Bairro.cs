using MeAjudaAi.Domain.Common;

namespace MeAjudaAi.Domain.Entities;

public class Bairro : EntityBase
{
    public Guid CidadeId { get; set; }
    public string Nome { get; set; } = string.Empty;

    public Cidade Cidade { get; set; } = null!;
    public ICollection<AreaAtendimento> AreasAtendimento { get; set; } = new List<AreaAtendimento>();
}