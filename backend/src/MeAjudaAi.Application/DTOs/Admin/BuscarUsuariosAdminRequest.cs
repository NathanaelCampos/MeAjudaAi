using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Admin;

public class BuscarUsuariosAdminRequest
{
    public string? Nome { get; set; }
    public string? Email { get; set; }
    public TipoPerfil? TipoPerfil { get; set; }
    public bool? Ativo { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 20;
}
