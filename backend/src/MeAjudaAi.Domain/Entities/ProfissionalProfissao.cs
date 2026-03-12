namespace MeAjudaAi.Domain.Entities;

public class ProfissionalProfissao
{
    public Guid ProfissionalId { get; set; }
    public Guid ProfissaoId { get; set; }

    public Profissional Profissional { get; set; } = null!;
    public Profissao Profissao { get; set; } = null!;
}