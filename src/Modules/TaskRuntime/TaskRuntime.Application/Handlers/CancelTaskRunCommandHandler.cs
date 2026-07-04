namespace TaskRuntime.Application.Handlers;

using Shared.Cqrs;
using Shared.Tasks;
using Shared.Runtime.Time;
using Shared.Results;
using TaskRuntime.Application.Commands;

internal sealed class CancelTaskRunCommandHandler(
    ITaskRunStore store,
    ISystemClock clock)
    : ICommandHandler<CancelTaskRunCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(
        CancelTaskRunCommand command,
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

        TaskRunStatus status = run.Summary.Status;
        if (status is TaskRunStatus.Canceled or TaskRunStatus.CancellationRequested)
        {
            return Result.Success(Unit.Value);
        }

        if (!TaskRunStatusTransitions.CanRequestCancellation(status))
        {
            return Result.Failure<Unit>(TaskRuntimeApplicationErrors.RunCannotBeCanceled);
        }

        await store.RequestCancellationAsync(
                command.RunId,
                command.RequestedBy,
                clock.UtcNow,
                cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(Unit.Value);
    }
}
