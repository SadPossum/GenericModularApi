namespace TaskSamples.Application;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shared.Application.Composition;
using Shared.Tasks;
using TaskSamples.Application.Tasks;
using TaskSamples.Contracts;

public static class DependencyInjection
{
    public static IServiceCollection AddTaskSamplesApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddApplicationServicesFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddTaskHandler<GenerateReportTaskPayload, GenerateReportTaskHandler>(
            TaskSamplesModuleMetadata.Name,
            TaskSamplesModuleMetadata.GenerateReportTaskName,
            TaskSamplesModuleMetadata.WorkerGroup,
            tenantScoped: true,
            payloadVersion: TaskSamplesModuleMetadata.GenerateReportTaskPayloadVersion,
            kind: ModuleTaskKind.OneShot);
        services.AddTaskHandler<GenerateReportTaskPayloadV2, GenerateReportTaskV2Handler>(
            TaskSamplesModuleMetadata.Name,
            TaskSamplesModuleMetadata.GenerateReportTaskName,
            TaskSamplesModuleMetadata.WorkerGroup,
            tenantScoped: true,
            payloadVersion: TaskSamplesModuleMetadata.GenerateReportTaskPayloadVersion2,
            kind: ModuleTaskKind.OneShot);
        services.AddTaskHandler<FlakyReportTaskPayload, FlakyReportTaskHandler>(
            TaskSamplesModuleMetadata.Name,
            TaskSamplesModuleMetadata.FlakyReportTaskName,
            TaskSamplesModuleMetadata.WorkerGroup,
            tenantScoped: true,
            kind: ModuleTaskKind.OneShot);
        services.AddTaskHandler<SlowReportTaskPayload, SlowReportTaskHandler>(
            TaskSamplesModuleMetadata.Name,
            TaskSamplesModuleMetadata.SlowReportTaskName,
            TaskSamplesModuleMetadata.WorkerGroup,
            tenantScoped: true,
            kind: ModuleTaskKind.OneShot,
            supportsControlMessages: true);
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ITaskScheduleProvider, TaskSamplesScheduleProvider>());
        services.TryAddSingleton<ITaskSampleReportSink, NullTaskSampleReportSink>();

        return services;
    }
}
