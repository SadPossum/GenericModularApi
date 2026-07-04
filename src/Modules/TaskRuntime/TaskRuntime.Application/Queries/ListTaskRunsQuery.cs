namespace TaskRuntime.Application.Queries;

using Shared.Cqrs;
using Shared.Tasks;

public sealed record ListTaskRunsQuery(
    string? ModuleName,
    string? TaskName,
    string? WorkerGroup,
    TaskRunStatus? Status,
    string? TenantId,
    int Page,
    int PageSize) : IQuery<IReadOnlyList<TaskRunSummary>>;
