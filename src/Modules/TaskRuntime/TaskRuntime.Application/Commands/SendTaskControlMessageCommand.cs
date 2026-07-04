namespace TaskRuntime.Application.Commands;

using Shared.Cqrs;
using Shared.Tasks;

public sealed record SendTaskControlMessageCommand(
    Guid RunId,
    string CommandName,
    string PayloadJson,
    DateTimeOffset? ExpiresAtUtc,
    string? RequestedBy) : ITransactionalCommand<TaskControlMessage>;
