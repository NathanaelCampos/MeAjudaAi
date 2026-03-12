using MeAjudaAi.Application.DTOs.Auth;

namespace MeAjudaAi.Application.Interfaces.Auth;

public interface IAuthService
{
    Task<AuthResponse> RegistrarAsync(RegistrarUsuarioRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}