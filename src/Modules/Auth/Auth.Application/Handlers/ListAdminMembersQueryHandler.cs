namespace Auth.Application.Handlers;

using Auth.Application.Ports;
using Auth.Application.Queries;
using Auth.Contracts;
using Shared.Cqrs;
using Shared.Pagination;
using Shared.Results;

internal sealed class ListAdminMembersQueryHandler(IAdminMemberReadRepository repository)
    : IQueryHandler<ListAdminMembersQuery, AdminMemberListResponse>
{
    public async Task<Result<AdminMemberListResponse>> HandleAsync(
        ListAdminMembersQuery query,
        CancellationToken cancellationToken)
    {
        PageRequest pageRequest = PageRequest.Normalize(query.Page, query.PageSize);

        return Result.Success(await repository.ListMembersAsync(pageRequest, cancellationToken).ConfigureAwait(false));
    }
}
