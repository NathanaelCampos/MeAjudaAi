using System.Security.Claims;

namespace MeAjudaAi.Api.Extensions;

public static class UserClaimsPrincipalExtensions
{
    public static Guid? ObterUsuarioId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(claim))
            return null;

        return Guid.TryParse(claim, out var usuarioId) ? usuarioId : null;
    }
}