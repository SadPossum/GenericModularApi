namespace Auth.Application.Commands;

using Auth.Contracts;
using Shared.Application.Cqrs;

public sealed record RefreshMemberSessionCommand(string AccessToken, string RefreshToken) : ITransactionalCommand<AuthTokensResponse>;
