namespace TaskSamples.Contracts;

using Shared.Tasks;
using Shared.Tenancy;

[TaskName(GenerateReportTaskPayload.TaskName)]
[TaskPayloadVersion(GenerateReportTaskPayload.PayloadVersion)]
[TaskDescription("Generate a sample tenant report through the task runtime.")]
[TaskKind(ModuleTaskKind.OneShot)]
[TaskWorkerGroup(TaskSamplesModuleMetadata.WorkerGroup)]
[TenantScoped]
public sealed record GenerateReportTaskPayload(string ReportName, int ExpectedRows) : ITaskPayload
{
    public const string TaskName = "generate-report";
    public const int PayloadVersion = 1;
}
