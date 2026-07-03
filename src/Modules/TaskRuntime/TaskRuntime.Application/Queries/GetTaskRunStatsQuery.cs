namespace TaskRuntime.Application.Queries;

using Shared.Application.Cqrs;
using Shared.Application.Tasks;

public sealed record GetTaskRunStatsQuery(
    string? ModuleName,
    string? TaskName,
    string? WorkerGroup,
    string? TenantId) : IQuery<TaskRunStats>;
