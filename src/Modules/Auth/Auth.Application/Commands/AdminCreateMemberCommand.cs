namespace Auth.Application.Commands;

using Auth.Contracts;
using Shared.Application.Cqrs;

public sealed record AdminCreateMemberCommand(string Username, UsernameType UsernameType, string Password)
    : ITransactionalCommand<AdminCreatedMemberResponse>;
