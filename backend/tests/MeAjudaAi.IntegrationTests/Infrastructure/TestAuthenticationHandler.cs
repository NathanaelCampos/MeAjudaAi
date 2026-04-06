using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using MeAjudaAi.Application.Common;
using MeAjudaAi.Domain.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace MeAjudaAi.IntegrationTests.Infrastructure;

public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public new const string Scheme = "IntegrationTest";
    public const string RoleHeader = "X-Integration-Test-Role";
    public const string RolesHeader = "X-Integration-Test-Roles";
    public const string UserIdHeader = "X-Integration-Test-UserId";
    public const string UserEmailHeader = "X-Integration-Test-Email";
    public const string UserNameHeader = "X-Integration-Test-UserName";
    public const string AnonymousRole = "anonymous";

    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var resolvedHeaderRoles = ParseRoles(Request.Headers[RolesHeader]);
        var legacyRole = Request.Headers[RoleHeader].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(legacyRole))
            resolvedHeaderRoles.Add(legacyRole);

        var hasRoleHeader = resolvedHeaderRoles.Count > 0;

        if (hasRoleHeader && resolvedHeaderRoles.Any(r => string.Equals(r, AnonymousRole, StringComparison.OrdinalIgnoreCase)))
            return Task.FromResult(AuthenticateResult.NoResult());

        var userIdHeader = Request.Headers[UserIdHeader].FirstOrDefault();
        var email = Request.Headers[UserEmailHeader].FirstOrDefault();
        var name = Request.Headers[UserNameHeader].FirstOrDefault();

        if (!hasRoleHeader)
        {
            if (!TryParseJwtFromAuthorization(out var jwtRole, out var jwtEmail, out var jwtUserId, out var jwtName))
                return Task.FromResult(AuthenticateResult.NoResult());

            if (!string.IsNullOrWhiteSpace(jwtRole))
                resolvedHeaderRoles.Add(jwtRole);
            email = email ?? jwtEmail;
            name = name ?? jwtName;
            userIdHeader = userIdHeader ?? jwtUserId?.ToString();
        }

        var resolvedRoles = resolvedHeaderRoles
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r!.Trim())
            .DefaultIfEmpty(AccessRoles.Cliente)
            .ToList();

        var primaryRole = resolvedRoles.First();
        email ??= "test@integration.local";
        name ??= "Integration Test";

        var userId = Guid.TryParse(userIdHeader, out var parsedId)
            ? parsedId
            : Guid.NewGuid();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, name),
            new(ClaimTypes.Email, email)
        };

        foreach (var resolvedRole in resolvedRoles.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            claims.Add(new Claim(ClaimTypes.Role, resolvedRole));
        }

        var identity = new ClaimsIdentity(claims, Scheme);
        var principal = new ClaimsPrincipal(identity);

        Logger.LogDebug("TestAuthenticationHandler: authenticated user {UserId} role={Role}", userId, primaryRole);

        var ticket = new AuthenticationTicket(principal, Scheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private static List<string> ParseRoles(StringValues values)
    {
        var roles = new List<string>();

        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
                continue;

            roles.AddRange(value
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Trim())
                .Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        return roles;
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
