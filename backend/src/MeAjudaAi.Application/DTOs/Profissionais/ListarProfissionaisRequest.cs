namespace MeAjudaAi.Application.DTOs.Profissionais;

public class ListarProfissionaisRequest
{
    public string? Nome { get; set; }
    public bool SomenteAtivos { get; set; } = true;
}