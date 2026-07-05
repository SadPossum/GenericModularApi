namespace Notifications.Application.Handlers;

using Notifications.Application.Queries;
using Notifications.Application.Ports;
using Notifications.Contracts;
using Shared.Cqrs;
using Shared.Pagination;
using Shared.Results;

internal sealed class ListTenantNotificationBroadcastsQueryHandler(INotificationBroadcastRepository repository)
    : IQueryHandler<ListTenantNotificationBroadcastsQuery, AdminNotificationBroadcastListResponse>
{
    public async Task<Result<AdminNotificationBroadcastListResponse>> HandleAsync(
        ListTenantNotificationBroadcastsQuery query,
        CancellationToken cancellationToken)
    {
        AdminNotificationBroadcastListResponse response = await repository
            .ListTenantBroadcastsAsync(
                query.TenantId,
                PageRequest.Normalize(query.Page, query.PageSize),
                cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(response);
    }
}
