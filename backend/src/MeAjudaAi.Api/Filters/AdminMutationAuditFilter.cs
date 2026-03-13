using System.Text.Json;
using MeAjudaAi.Api.Extensions;
using MeAjudaAi.Application.Interfaces.Admin;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace MeAjudaAi.Api.Filters;

public class AdminMutationAuditFilter : IAsyncActionFilter
{
    private static readonly HashSet<string> MetodosAuditaveis = ["POST", "PUT", "PATCH", "DELETE"];
    private readonly IAdminAuditoriaService _adminAuditoriaService;

    public AdminMutationAuditFilter(IAdminAuditoriaService adminAuditoriaService)
    {
        _adminAuditoriaService = adminAuditoriaService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executedContext = await next();

        var httpContext = context.HttpContext;
        if (!MetodosAuditaveis.Contains(httpContext.Request.Method))
            return;

        if (!httpContext.User.IsInRole("Administrador"))
            return;

        if (executedContext.Exception is not null && !executedContext.ExceptionHandled)
            return;

        var statusCode = ObterStatusCode(executedContext);
        if (statusCode >= 400)
            return;

        var path = httpContext.Request.Path.Value ?? string.Empty;
        if (DeveIgnorar(path))
            return;

        var adminUsuarioId = httpContext.User.ObterUsuarioId();
        if (adminUsuarioId is null)
            return;

        var controller = context.ActionDescriptor.RouteValues.TryGetValue("controller", out var controllerValue)
            ? controllerValue?.ToLowerInvariant() ?? "admin"
            : "admin";
        var action = context.ActionDescriptor.RouteValues.TryGetValue("action", out var actionValue)
            ? actionValue?.ToLowerInvariant() ?? "executar"
            : "executar";
        var entidade = MapearEntidade(controller);
        var entidadeId = ObterEntidadeId(context.RouteData.Values);
        var descricao = Limitar($"Administrador executou {action} em {entidade}.", 300);
        var payloadJson = JsonSerializer.Serialize(new
        {
            metodo = httpContext.Request.Method,
            path,
            queryString = httpContext.Request.QueryString.Value,
            controller,
            action,
            statusCode
        });

        await _adminAuditoriaService.RegistrarAsync(
            adminUsuarioId.Value,
            Limitar(entidade, 100),
            entidadeId,
            Limitar(action, 100),
            descricao,
            Limitar(payloadJson, 4000),
            httpContext.RequestAborted);
    }

    private static int ObterStatusCode(ActionExecutedContext context)
    {
        if (context.Result is IStatusCodeActionResult statusCodeActionResult && statusCodeActionResult.StatusCode.HasValue)
            return statusCodeActionResult.StatusCode.Value;

        return context.HttpContext.Response.StatusCode == 0
            ? StatusCodes.Status200OK
            : context.HttpContext.Response.StatusCode;
    }

    private static bool DeveIgnorar(string path)
    {
        return path.StartsWith("/api/admin/usuarios/", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("/api/admin/profissionais/", StringComparison.OrdinalIgnoreCase);
    }

    private static string MapearEntidade(string controller)
    {
        return controller switch
        {
            "notificacoes" => "notificacao",
            "avaliacoes" => "avaliacao",
            "impulsionamentos" => "impulsionamento",
            "importacao" => "importacao",
            _ => controller
        };
    }

    private static Guid ObterEntidadeId(RouteValueDictionary routeValues)
    {
        foreach (var key in new[]
                 {
                     "usuarioId",
                     "profissionalId",
                     "servicoId",
                     "avaliacaoId",
                     "impulsionamentoId",
                     "webhookId",
                     "notificacaoId",
                     "auditoriaId",
                     "id"
                 })
        {
            if (routeValues.TryGetValue(key, out var value) &&
                value is not null &&
                Guid.TryParse(value.ToString(), out var guid))
            {
                return guid;
            }
        }

        return Guid.Empty;
    }

    private static string Limitar(string valor, int maxLength)
    {
        return valor.Length <= maxLength
            ? valor
            : valor[..maxLength];
    }
}
