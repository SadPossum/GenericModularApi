namespace TaskRuntime.Application.Commands;

using Shared.Application;
using Shared.Application.Cqrs;

public sealed record CancelTaskRunCommand(
    Guid RunId,
    string? RequestedBy) : ITransactionalCommand<Unit>;
