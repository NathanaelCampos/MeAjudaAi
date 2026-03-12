using System.Text.Json;

namespace MeAjudaAi.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (InvalidOperationException ex)
        {
            await EscreverRespostaAsync(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            await EscreverRespostaAsync(context, StatusCodes.Status401Unauthorized, ex.Message);
        }
    }

    private static async Task EscreverRespostaAsync(
        HttpContext context,
        int statusCode,
        string mensagem)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var payload = JsonSerializer.Serialize(new
        {
            mensagem
        });

        await context.Response.WriteAsync(payload);
    }
}
