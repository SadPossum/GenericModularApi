namespace TaskRuntime.Application;

using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Composition;

public static class DependencyInjection
{
    public static IServiceCollection AddTaskRuntimeApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddApplicationServicesFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
