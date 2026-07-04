namespace TaskRuntime.Application.Handlers;

using Shared.Cqrs;
using Shared.Tasks;
using Shared.Results;
using TaskRuntime.Application.Queries;

internal sealed class GetTaskRunQueryHandler(ITaskRunStore store)
    : IQueryHandler<GetTaskRunQuery, TaskRunDetails>
{
    public async Task<Result<TaskRunDetails>> HandleAsync(
        GetTaskRunQuery query,
        CancellationToken cancellationToken)
    {
        if (query.RunId == Guid.Empty)
        {
            return Result.Failure<TaskRunDetails>(TaskRuntimeApplicationErrors.InvalidRunId);
        }

        TaskRunDetails? run = await store.GetAsync(query.RunId, cancellationToken).ConfigureAwait(false);
        return run is null
            ? Result.Failure<TaskRunDetails>(TaskRuntimeApplicationErrors.RunNotFound)
            : Result.Success(run);
    }
}
