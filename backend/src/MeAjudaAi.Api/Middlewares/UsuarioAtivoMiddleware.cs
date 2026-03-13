using MeAjudaAi.Api.Extensions;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Api.Middlewares;

public class UsuarioAtivoMiddleware
{
    private readonly RequestDelegate _next;

    public UsuarioAtivoMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var usuarioId = context.User.ObterUsuarioId();

            if (usuarioId.HasValue)
            {
                var ativo = await dbContext.Usuarios
                    .AsNoTracking()
                    .Where(x => x.Id == usuarioId.Value)
                    .Select(x => (bool?)x.Ativo)
                    .FirstOrDefaultAsync(context.RequestAborted);

                if (ativo == false)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new MensagemErroResponse
                    {
                        Mensagem = "Usuário inativo."
                    }, context.RequestAborted);
                    return;
                }
            }
        }

        await _next(context);
    }
}
