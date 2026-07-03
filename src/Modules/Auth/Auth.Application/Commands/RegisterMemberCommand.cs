namespace Auth.Application.Commands;

using Auth.Contracts;
using Shared.Application.Cqrs;

public sealed record RegisterMemberCommand(string Username, UsernameType UsernameType, string Password)
    : ITransactionalCommand<AuthTokensResponse>;
