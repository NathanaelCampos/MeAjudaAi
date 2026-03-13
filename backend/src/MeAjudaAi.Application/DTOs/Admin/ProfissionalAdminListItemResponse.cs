namespace MeAjudaAi.Application.DTOs.Admin;

public class ProfissionalAdminListItemResponse
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string NomeExibicao { get; set; } = string.Empty;
    public string NomeUsuario { get; set; } = string.Empty;
    public string EmailUsuario { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public bool PerfilVerificado { get; set; }
    public bool AceitaContatoPeloApp { get; set; }
    public DateTime DataCriacao { get; set; }
}
