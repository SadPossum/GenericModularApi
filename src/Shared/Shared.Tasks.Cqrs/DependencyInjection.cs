namespace Shared.Tasks.Cqrs;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shared.ModuleComposition;
using Shared.Cqrs.Infrastructure;
using Shared.Tasks;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddTaskCqrs(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddCqrsInfrastructure();
        builder.ProvideFeature(TasksCompositionFeatures.CqrsDispatcherProvided("Shared.Tasks.Cqrs"));
        builder.Services.TryAddScoped<ITaskCommandDispatcher, TaskCommandDispatcher>();

        return builder;
    }
}
