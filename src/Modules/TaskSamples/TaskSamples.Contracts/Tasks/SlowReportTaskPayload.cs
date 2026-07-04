namespace TaskSamples.Contracts;

using Shared.Tasks;
using Shared.Tenancy;

[TaskName(SlowReportTaskPayload.TaskName)]
[TaskPayloadVersion(SlowReportTaskPayload.PayloadVersion)]
[TaskDescription("Demonstrate long-running task progress, heartbeat reporting, and cooperative control.")]
[TaskKind(ModuleTaskKind.OneShot)]
[TaskWorkerGroup(TaskSamplesModuleMetadata.WorkerGroup)]
[SupportsTaskControl]
[TenantScoped]
public sealed record SlowReportTaskPayload(
    string ReportName,
    int ExpectedRows,
    int Steps,
    int DelayMilliseconds) : ITaskPayload
{
    public const string TaskName = "slow-report";
    public const int PayloadVersion = 1;
}
