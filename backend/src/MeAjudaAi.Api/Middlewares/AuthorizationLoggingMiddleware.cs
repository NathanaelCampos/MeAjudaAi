using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MeAjudaAi.Api.Middlewares;

public class AuthorizationLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthorizationLoggingMiddleware> _logger;

    public AuthorizationLoggingMiddleware(RequestDelegate next, ILogger<AuthorizationLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

        public async Task InvokeAsync(HttpContext context)
        {
            var hasAuth = context.Request.Headers.TryGetValue("Authorization", out var token);

            if (hasAuth)
            {
                _logger.LogInformation("Authorization header: {Authorization}", token.ToString());
            }
            else
            {
                _logger.LogInformation("Authorization header missing");
            }

            await _next(context);

            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
                _logger.LogWarning(
                    "401 Unauthorized: {Method} {Path}; Authorization header present={HasAuth}; userId={UserId}",
                    context.Request.Method,
                    context.Request.Path,
                    hasAuth,
                    userId);
            }
        }
}
