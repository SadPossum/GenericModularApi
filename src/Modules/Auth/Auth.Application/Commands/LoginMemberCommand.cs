namespace Auth.Application.Commands;

using Auth.Contracts;
using Shared.Cqrs;

public sealed record LoginMemberCommand(string Username, string Password) : ITransactionalCommand<AuthTokensResponse>;
