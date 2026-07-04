namespace TaskRuntime.Application.Handlers;

using Shared.Cqrs;
using Shared.Tasks;
using Shared.Results;
using TaskRuntime.Application.Queries;

internal sealed class ListTaskRunsQueryHandler(ITaskRunStore store)
    : IQueryHandler<ListTaskRunsQuery, IReadOnlyList<TaskRunSummary>>
{
    public async Task<Result<IReadOnlyList<TaskRunSummary>>> HandleAsync(
        ListTaskRunsQuery query,
        CancellationToken cancellationToken)
    {
        TaskRunFilter filter = new(
            query.ModuleName,
            query.TaskName,
            query.WorkerGroup,
            query.Status,
            query.TenantId,
            query.Page,
            query.PageSize);

        IReadOnlyList<TaskRunSummary> runs = await store.ListAsync(filter, cancellationToken).ConfigureAwait(false);
        return Result.Success(runs);
    }
}
