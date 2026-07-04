namespace TaskRuntime.Application.Queries;

using Shared.Cqrs;
using Shared.Tasks;

public sealed record GetTaskRunStatsQuery(
    string? ModuleName,
    string? TaskName,
    string? WorkerGroup,
    string? TenantId) : IQuery<TaskRunStats>;
