namespace Shared.Tasks.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shared.Cqrs.Infrastructure;
using Shared.Observability.Infrastructure;
using Shared.Runtime.Infrastructure;
using Shared.Tasks;
using Shared.Tasks.Cqrs;

public static class TaskWorkerRuntimeDependencyInjection
{
    public static IHostApplicationBuilder AddTaskInfrastructure(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddRuntimeInfrastructure();
        builder.AddCqrsInfrastructure();

        if (builder.Services.Any(descriptor => descriptor.ServiceType == typeof(TaskInfrastructureRegistrationMarker)))
        {
            return builder;
        }

        builder.Services.AddSingleton<TaskInfrastructureRegistrationMarker>();
        builder.Services.TryAddScoped<ITaskCommandDispatcher, TaskCommandDispatcher>();
        builder.Services.TryAddScoped<ITaskControlLoop, TaskControlLoop>();
        builder.Services.TryAddSingleton<TaskMetricsSnapshotStore>();
        builder.Services.TryAddSingleton<TaskMetrics>();

        return builder;
    }

    public static IHostApplicationBuilder AddTaskWorkerRuntime(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddTaskInfrastructure();
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

        builder.AddTaskInfrastructure();
        builder.Services
            .AddOptions<TaskRunSchedulerOptions>()
            .Bind(builder.Configuration.GetSection(TaskRunSchedulerOptions.SectionName))
            .ValidateOnStart();
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<TaskRunSchedulerOptions>, TaskRunSchedulerOptionsValidator>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, TaskRunSchedulerService>());

        return builder;
    }

    private sealed class TaskInfrastructureRegistrationMarker;
}
