using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Auth;

public class RegistrarUsuarioRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public TipoPerfil TipoPerfil { get; set; }
}