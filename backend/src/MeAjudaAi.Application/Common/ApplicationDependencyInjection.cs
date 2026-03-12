using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Application.Common;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}