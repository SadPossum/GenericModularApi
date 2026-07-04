namespace Shared.Tasks.Cqrs;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shared.Cqrs.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddTaskCqrs(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddCqrsInfrastructure();
        builder.Services.TryAddScoped<ITaskCommandDispatcher, TaskCommandDispatcher>();

        return builder;
    }
}
