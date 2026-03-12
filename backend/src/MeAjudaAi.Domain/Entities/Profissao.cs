using MeAjudaAi.Domain.Common;

namespace MeAjudaAi.Domain.Entities;

public class Profissao : EntityBase
{
    public string Nome { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public ICollection<Especialidade> Especialidades { get; set; } = new List<Especialidade>();
    public ICollection<ProfissionalProfissao> Profissionais { get; set; } = new List<ProfissionalProfissao>();
}