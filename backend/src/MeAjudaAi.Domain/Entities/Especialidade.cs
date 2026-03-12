using MeAjudaAi.Domain.Common;

namespace MeAjudaAi.Domain.Entities;

public class Especialidade : EntityBase
{
    public Guid ProfissaoId { get; set; }
    public string Nome { get; set; } = string.Empty;

    public Profissao Profissao { get; set; } = null!;
    public ICollection<ProfissionalEspecialidade> Profissionais { get; set; } = new List<ProfissionalEspecialidade>();
}