using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using MeAjudaAi.Application.Common;
using MeAjudaAi.Domain.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.IntegrationTests.Infrastructure;

public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public new const string Scheme = "IntegrationTest";
    public const string RoleHeader = "X-Integration-Test-Role";
    public const string UserIdHeader = "X-Integration-Test-UserId";
    public const string UserEmailHeader = "X-Integration-Test-Email";
    public const string UserNameHeader = "X-Integration-Test-UserName";
    public const string AnonymousRole = "anonymous";
    private const string DefaultRole = AccessRoles.Cliente;

    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var role = Request.Headers[RoleHeader].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(role) && string.Equals(role, AnonymousRole, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(AuthenticateResult.NoResult());
        var userIdHeader = Request.Headers[UserIdHeader].FirstOrDefault();
        var email = Request.Headers[UserEmailHeader].FirstOrDefault();
        var name = Request.Headers[UserNameHeader].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(role) && TryParseJwtFromAuthorization(out var jwtRole, out var jwtEmail, out var jwtUserId, out var jwtName))
        {
            role = jwtRole;
            email = email ?? jwtEmail;
            name = name ?? jwtName;
            userIdHeader = userIdHeader ?? jwtUserId?.ToString();
        }

        role ??= DefaultRole;
        email ??= "test@integration.local";
        name ??= "Integration Test";

        var userId = Guid.TryParse(userIdHeader, out var parsedId)
            ? parsedId
            : Guid.NewGuid();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, name),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, Scheme);
        var principal = new ClaimsPrincipal(identity);

        Logger.LogDebug("TestAuthenticationHandler: authenticated user {UserId} role={Role}", userId, role);

        var ticket = new AuthenticationTicket(principal, Scheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private bool TryParseJwtFromAuthorization(out string? role, out string? email, out Guid? userId, out string? name)
    {
        role = null;
        email = null;
        name = null;
        userId = null;

        if (!Request.Headers.TryGetValue("Authorization", out var values))
            return false;

        var header = values.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(header) || !header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return false;

        var token = header.Substring("Bearer ".Length).Trim();
        if (string.IsNullOrEmpty(token))
            return false;

        try
        {
            var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(token);
            role = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            email = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            name = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var idValue = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(idValue, out var parsed))
                userId = parsed;
            return true;
        }
        catch
        {
            return false;
        }
    }
}
