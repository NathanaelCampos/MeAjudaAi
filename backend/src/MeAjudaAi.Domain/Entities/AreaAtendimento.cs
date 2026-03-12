using MeAjudaAi.Domain.Common;

namespace MeAjudaAi.Domain.Entities;

public class AreaAtendimento : EntityBase
{
    public Guid ProfissionalId { get; set; }
    public Guid CidadeId { get; set; }
    public Guid? BairroId { get; set; }
    public bool CidadeInteira { get; set; }

    public Profissional Profissional { get; set; } = null!;
    public Cidade Cidade { get; set; } = null!;
    public Bairro? Bairro { get; set; }
}