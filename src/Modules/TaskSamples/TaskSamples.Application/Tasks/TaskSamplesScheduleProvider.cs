namespace TaskSamples.Application.Tasks;

using System.Text.Json;
using Shared.Tasks;
using TaskSamples.Contracts;

internal sealed class TaskSamplesScheduleProvider : ITaskScheduleProvider
{
    private static readonly string PayloadJson = JsonSerializer.Serialize(new GenerateReportTaskPayload("scheduled-daily", 10));

    public Task<IReadOnlyList<ScheduledTaskDefinition>> GetSchedulesAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ScheduledTaskDefinition>>(
        [
            new ScheduledTaskDefinition(
                "scheduled-report",
                TaskSamplesModuleMetadata.Name,
                TaskSamplesModuleMetadata.GenerateReportTaskName,
                PayloadJson,
                TimeSpan.FromMinutes(5),
                TaskSamplesModuleMetadata.WorkerGroup,
                tenantId: "default",
                maxAttempts: 3,
                payloadVersion: TaskSamplesModuleMetadata.GenerateReportTaskPayloadVersion)
        ]);
}
