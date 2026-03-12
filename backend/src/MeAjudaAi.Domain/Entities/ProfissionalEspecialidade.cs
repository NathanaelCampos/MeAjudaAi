namespace MeAjudaAi.Domain.Entities;

public class ProfissionalEspecialidade
{
    public Guid ProfissionalId { get; set; }
    public Guid EspecialidadeId { get; set; }

    public Profissional Profissional { get; set; } = null!;
    public Especialidade Especialidade { get; set; } = null!;
}