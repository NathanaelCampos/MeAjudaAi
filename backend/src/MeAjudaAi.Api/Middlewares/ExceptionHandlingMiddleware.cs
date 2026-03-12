using System.Text.Json;
using MeAjudaAi.Application.DTOs.Common;

namespace MeAjudaAi.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado durante o processamento da requisição {Method} {Path}.", context.Request.Method, context.Request.Path);
            await EscreverRespostaAsync(context, StatusCodes.Status500InternalServerError, "Erro interno.");
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

        var payload = JsonSerializer.Serialize(new MensagemErroResponse
        {
            Mensagem = mensagem
        });

        await context.Response.WriteAsync(payload);
    }
}
