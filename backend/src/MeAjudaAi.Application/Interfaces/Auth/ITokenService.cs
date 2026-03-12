using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Domain.Entities;

namespace MeAjudaAi.Application.Interfaces.Auth;

public interface ITokenService
{
    AuthResponse GerarToken(Usuario usuario);
}