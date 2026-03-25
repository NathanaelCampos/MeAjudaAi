using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.IntegrationTests.Infrastructure;

internal static class TestAuthenticationExtensions
{
    public static void ApplyTestAuthentication(this HttpClient client, AuthResponse auth, TipoPerfil profile)
    {
        client.DefaultRequestHeaders.Remove(TestAuthenticationHandler.UserIdHeader);
        client.DefaultRequestHeaders.Remove(TestAuthenticationHandler.UserEmailHeader);
        client.DefaultRequestHeaders.Remove(TestAuthenticationHandler.UserNameHeader);
        client.DefaultRequestHeaders.Remove(TestAuthenticationHandler.RoleHeader);

        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.UserIdHeader, auth.UsuarioId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.UserEmailHeader, auth.Email);
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.UserNameHeader, string.IsNullOrWhiteSpace(auth.Nome) ? auth.Email : auth.Nome);
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.RoleHeader, profile.ToString());
    }

    public static void ApplyAnonymous(this HttpClient client)
    {
        client.DefaultRequestHeaders.Remove(TestAuthenticationHandler.RoleHeader);
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.RoleHeader, TestAuthenticationHandler.AnonymousRole);
    }
}
