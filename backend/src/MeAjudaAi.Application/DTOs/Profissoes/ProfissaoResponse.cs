namespace MeAjudaAi.Application.DTOs.Profissoes;

public class ProfissaoResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}