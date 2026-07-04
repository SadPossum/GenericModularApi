namespace TaskRuntime.Application.Commands;

using Shared.Cqrs;

public sealed record RetryTaskRunCommand(
    Guid RunId,
    string? RequestedBy,
    DateTimeOffset? ScheduledAtUtc) : ITransactionalCommand<Unit>;
