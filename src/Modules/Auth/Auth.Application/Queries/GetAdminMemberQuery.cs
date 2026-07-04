namespace Auth.Application.Queries;

using Auth.Contracts;
using Shared.Cqrs;

public sealed record GetAdminMemberQuery(Guid MemberId) : IQuery<AdminMemberDetails>;
