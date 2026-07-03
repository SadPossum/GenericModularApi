namespace Shared.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Tasks;

public static class TaskWorkerRuntimeDependencyInjection
{
    public static IHostApplicationBuilder AddTaskWorkerRuntime(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddSharedInfrastructure();
        builder.Services
            .AddOptions<TaskWorkerOptions>()
            .Bind(builder.Configuration.GetSection(TaskWorkerOptions.SectionName))
            .ValidateOnStart();
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<TaskWorkerOptions>, TaskWorkerOptionsValidator>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, TaskWorkerService>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, TaskTimeoutScannerService>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, TaskMetricsSamplerService>());

        return builder;
    }

    public static IHostApplicationBuilder AddTaskRunScheduling(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddSharedInfrastructure();
        builder.Services
            .AddOptions<TaskRunSchedulerOptions>()
            .Bind(builder.Configuration.GetSection(TaskRunSchedulerOptions.SectionName))
            .ValidateOnStart();
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<TaskRunSchedulerOptions>, TaskRunSchedulerOptionsValidator>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, TaskRunSchedulerService>());

        return builder;
    }
}
