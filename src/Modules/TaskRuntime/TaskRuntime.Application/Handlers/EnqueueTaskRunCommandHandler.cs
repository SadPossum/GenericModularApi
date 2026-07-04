namespace TaskRuntime.Application.Handlers;

using System.Text.Json;
using Shared.Cqrs;
using Shared.Runtime.Identity;
using Shared.Tasks;
using Shared.Runtime.Time;
using Shared.Results;
using TaskRuntime.Application.Commands;

internal sealed class EnqueueTaskRunCommandHandler(
    ITaskRunStore store,
    IIdGenerator idGenerator,
    ISystemClock clock)
    : ICommandHandler<EnqueueTaskRunCommand, TaskRunDetails>
{
    public async Task<Result<TaskRunDetails>> HandleAsync(
        EnqueueTaskRunCommand command,
        CancellationToken cancellationToken)
    {
        if (!IsValidJson(command.PayloadJson))
        {
            return Result.Failure<TaskRunDetails>(TaskRuntimeApplicationErrors.InvalidPayloadJson);
        }

        DateTimeOffset nowUtc = clock.UtcNow;
        Guid runId = command.RunId.GetValueOrDefault();
        if (runId == Guid.Empty)
        {
            runId = idGenerator.NewId();
        }

        DateTimeOffset scheduledAtUtc = command.ScheduledAtUtc ?? nowUtc;
        TaskRunRequest request = new(
            runId,
            command.ModuleName,
            command.TaskName,
            command.PayloadJson,
            nowUtc,
            scheduledAtUtc,
            command.WorkerGroup,
            command.TenantId,
            command.CorrelationId,
            command.RequestedBy,
            command.MaxAttempts,
            command.PayloadVersion,
            command.DeduplicationKey);

        await store.EnqueueAsync(request, cancellationToken).ConfigureAwait(false);

        TaskRunDetails? run = await store.GetAsync(runId, cancellationToken).ConfigureAwait(false);
        return run is null
            ? Result.Failure<TaskRunDetails>(TaskRuntimeApplicationErrors.RunNotFound)
            : Result.Success(run);
    }

    private static bool IsValidJson(string payloadJson)
    {
        try
        {
            using JsonDocument _ = JsonDocument.Parse(payloadJson);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
