namespace TaskRuntime.Application.Commands;

using Shared.Application;
using Shared.Application.Cqrs;

public sealed record RetryTaskRunCommand(
    Guid RunId,
    string? RequestedBy,
    DateTimeOffset? ScheduledAtUtc) : ITransactionalCommand<Unit>;
