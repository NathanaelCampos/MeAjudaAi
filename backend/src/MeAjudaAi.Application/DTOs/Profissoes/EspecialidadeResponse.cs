namespace MeAjudaAi.Application.DTOs.Profissoes;

public class EspecialidadeResponse
{
    public Guid Id { get; set; }
    public Guid ProfissaoId { get; set; }
    public string Nome { get; set; } = string.Empty;
}