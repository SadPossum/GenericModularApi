namespace Administration.Application.Handlers;

using Administration.Application.Ports;
using Administration.Application.Queries;
using Shared.Cqrs;
using Shared.Results;

internal sealed class ListRolesQueryHandler(IAdminRbacRepository repository)
    : IQueryHandler<ListRolesQuery, IReadOnlyList<AdminRoleDetails>>
{
    public async Task<Result<IReadOnlyList<AdminRoleDetails>>> HandleAsync(
        ListRolesQuery query,
        CancellationToken cancellationToken) =>
        Result.Success(await repository.ListRolesAsync(cancellationToken).ConfigureAwait(false));
}
