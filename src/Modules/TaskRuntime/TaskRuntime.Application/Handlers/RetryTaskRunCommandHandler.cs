namespace TaskRuntime.Application.Handlers;

using Shared.Cqrs;
using Shared.Tasks;
using Shared.Runtime.Time;
using Shared.Results;
using TaskRuntime.Application.Commands;

internal sealed class RetryTaskRunCommandHandler(
    ITaskRunStore store,
    ISystemClock clock)
    : ICommandHandler<RetryTaskRunCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(
        RetryTaskRunCommand command,
        CancellationToken cancellationToken)
    {
        if (command.RunId == Guid.Empty)
        {
            return Result.Failure<Unit>(TaskRuntimeApplicationErrors.InvalidRunId);
        }

        TaskRunDetails? run = await store.GetAsync(command.RunId, cancellationToken).ConfigureAwait(false);
        if (run is null)
        {
            return Result.Failure<Unit>(TaskRuntimeApplicationErrors.RunNotFound);
        }

        if (!CanRetry(run.Summary.Status))
        {
            return Result.Failure<Unit>(TaskRuntimeApplicationErrors.RunCannotBeRetried);
        }

        await store.RetryAsync(
                command.RunId,
                command.RequestedBy,
                command.ScheduledAtUtc ?? clock.UtcNow,
                cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(Unit.Value);
    }

    private static bool CanRetry(TaskRunStatus status) =>
        TaskRunStatusTransitions.RequireKnown(status) is TaskRunStatus.Failed or
            TaskRunStatus.TimedOut or
            TaskRunStatus.Canceled or
            TaskRunStatus.RetryScheduled;
}
