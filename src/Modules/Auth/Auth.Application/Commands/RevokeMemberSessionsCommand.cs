namespace Auth.Application.Commands;

using Auth.Contracts;
using Shared.Cqrs;

public sealed record RevokeMemberSessionsCommand(Guid MemberId) : ITransactionalCommand<AdminRevokeSessionsResponse>;
