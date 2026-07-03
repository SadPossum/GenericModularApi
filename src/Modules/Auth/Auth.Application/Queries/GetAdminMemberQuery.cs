namespace Auth.Application.Queries;

using Auth.Contracts;
using Shared.Application.Cqrs;

public sealed record GetAdminMemberQuery(Guid MemberId) : IQuery<AdminMemberDetails>;
