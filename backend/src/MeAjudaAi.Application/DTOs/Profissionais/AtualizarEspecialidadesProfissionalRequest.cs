namespace MeAjudaAi.Application.DTOs.Profissionais;

public class AtualizarEspecialidadesProfissionalRequest
{
    public List<Guid> EspecialidadeIds { get; set; } = new();
}