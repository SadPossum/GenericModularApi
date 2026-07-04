namespace Auth.Application.Commands;

using Auth.Contracts;
using Shared.Cqrs;

public sealed record AdminCreateMemberCommand(string Username, UsernameType UsernameType, string Password)
    : ITransactionalCommand<AdminCreatedMemberResponse>;
