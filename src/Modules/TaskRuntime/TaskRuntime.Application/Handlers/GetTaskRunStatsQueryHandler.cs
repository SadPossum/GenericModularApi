namespace TaskRuntime.Application.Handlers;

using Shared.Cqrs;
using Shared.Tasks;
using Shared.Results;
using TaskRuntime.Application.Queries;

internal sealed class GetTaskRunStatsQueryHandler(ITaskRunStore store)
    : IQueryHandler<GetTaskRunStatsQuery, TaskRunStats>
{
    public async Task<Result<TaskRunStats>> HandleAsync(
        GetTaskRunStatsQuery query,
        CancellationToken cancellationToken)
    {
        TaskRunStats stats = await store.GetStatsAsync(
                new TaskRunStatsFilter(
                    query.ModuleName,
                    query.TaskName,
                    query.WorkerGroup,
                    query.TenantId),
                cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(stats);
    }
}
