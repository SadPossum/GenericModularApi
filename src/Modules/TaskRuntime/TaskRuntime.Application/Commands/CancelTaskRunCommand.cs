namespace TaskRuntime.Application.Commands;

using Shared.Cqrs;

public sealed record CancelTaskRunCommand(
    Guid RunId,
    string? RequestedBy) : ITransactionalCommand<Unit>;
