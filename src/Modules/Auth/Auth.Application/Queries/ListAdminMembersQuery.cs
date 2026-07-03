namespace Auth.Application.Queries;

using Auth.Contracts;
using Shared.Application.Cqrs;

public sealed record ListAdminMembersQuery(int Page, int PageSize) : IQuery<AdminMemberListResponse>;
