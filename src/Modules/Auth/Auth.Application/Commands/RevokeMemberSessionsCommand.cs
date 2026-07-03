namespace Auth.Application.Commands;

using Auth.Contracts;
using Shared.Application.Cqrs;

public sealed record RevokeMemberSessionsCommand(Guid MemberId) : ITransactionalCommand<AdminRevokeSessionsResponse>;
