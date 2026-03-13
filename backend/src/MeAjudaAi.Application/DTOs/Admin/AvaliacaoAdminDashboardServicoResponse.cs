namespace MeAjudaAi.Application.DTOs.Admin;

public class AvaliacaoAdminDashboardServicoResponse
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public Guid ClienteId { get; set; }
    public Guid ProfissionalId { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public string NomeProfissional { get; set; } = string.Empty;
    public string? NomeProfissao { get; set; }
    public string? NomeEspecialidade { get; set; }
}
