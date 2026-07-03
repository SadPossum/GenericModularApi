namespace TaskRuntime.Application.Commands;

using Shared.Application.Cqrs;
using Shared.Application.Tasks;

public sealed record SendTaskControlMessageCommand(
    Guid RunId,
    string CommandName,
    string PayloadJson,
    DateTimeOffset? ExpiresAtUtc,
    string? RequestedBy) : ITransactionalCommand<TaskControlMessage>;
