namespace MeAjudaAi.Application.DTOs.Profissionais;

public class AtualizarProfissoesProfissionalRequest
{
    public List<Guid> ProfissaoIds { get; set; } = new();
}