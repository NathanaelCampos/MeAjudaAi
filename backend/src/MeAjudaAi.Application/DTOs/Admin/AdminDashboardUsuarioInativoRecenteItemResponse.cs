using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Admin;

public class AdminDashboardUsuarioInativoRecenteItemResponse
{
    public Guid UsuarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public TipoPerfil TipoPerfil { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}
