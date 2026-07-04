namespace TaskRuntime.Application.Commands;

using Shared.Cqrs;
using Shared.Tasks;

public sealed record EnqueueTaskRunCommand(
    Guid? RunId,
    string ModuleName,
    string TaskName,
    string PayloadJson,
    DateTimeOffset? ScheduledAtUtc,
    string WorkerGroup,
    string? TenantId,
    Guid? CorrelationId,
    string? RequestedBy,
    int MaxAttempts,
    int PayloadVersion,
    string? DeduplicationKey) : ITransactionalCommand<TaskRunDetails>;
