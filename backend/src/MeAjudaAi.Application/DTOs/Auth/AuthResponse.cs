namespace MeAjudaAi.Application.DTOs.Auth;

public class AuthResponse
{
    public Guid UsuarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEmUtc { get; set; }
}