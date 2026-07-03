namespace TaskSamples.Contracts;

using Shared.Application.Modules;

public static class TaskSamplesModuleMetadata
{
    public const string Name = "task-samples";
    public const string GenerateReportTaskName = "generate-report";
    public const int GenerateReportTaskPayloadVersion = 1;
    public const int GenerateReportTaskPayloadVersion2 = 2;
    public const string FlakyReportTaskName = "flaky-report";
    public const string SlowReportTaskName = "slow-report";
    public const string WorkerGroup = "samples";

    public static ModuleDescriptor Descriptor { get; } = new(
        Name,
        schema: null,
        [],
        [],
        [],
        [],
        tasks:
        [
            new ModuleTaskDescriptor(
                GenerateReportTaskName,
                "Generate a sample tenant report through the task runtime.",
                ModuleTaskKind.OneShot,
                tenantScoped: true,
                supportsControlMessages: false,
                WorkerGroup,
                GenerateReportTaskPayloadVersion),
            new ModuleTaskDescriptor(
                GenerateReportTaskName,
                "Generate a sample tenant report through the task runtime using the v2 payload contract.",
                ModuleTaskKind.OneShot,
                tenantScoped: true,
                supportsControlMessages: false,
                WorkerGroup,
                GenerateReportTaskPayloadVersion2),
            new ModuleTaskDescriptor(
                FlakyReportTaskName,
                "Demonstrate retry behavior by failing until a configured attempt.",
                ModuleTaskKind.OneShot,
                tenantScoped: true,
                supportsControlMessages: false,
                WorkerGroup),
            new ModuleTaskDescriptor(
                SlowReportTaskName,
                "Demonstrate long-running task progress, heartbeat reporting, and cooperative control.",
                ModuleTaskKind.OneShot,
                tenantScoped: true,
                supportsControlMessages: true,
                WorkerGroup)
        ]);
}
