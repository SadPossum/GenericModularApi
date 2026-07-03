namespace Auth.Application.Commands;

using Auth.Contracts;
using Shared.Application.Cqrs;

public sealed record LoginMemberCommand(string Username, string Password) : ITransactionalCommand<AuthTokensResponse>;
