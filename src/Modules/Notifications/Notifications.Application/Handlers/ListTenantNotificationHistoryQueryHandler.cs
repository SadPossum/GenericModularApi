namespace Notifications.Application.Handlers;

using Notifications.Application.Ports;
using Notifications.Application.Queries;
using Notifications.Contracts;
using Shared.Cqrs;
using Shared.Pagination;
using Shared.Results;

internal sealed class ListTenantNotificationHistoryQueryHandler(INotificationHistoryRepository repository)
    : IQueryHandler<ListTenantNotificationHistoryQuery, AdminNotificationHistoryListResponse>
{
    public async Task<Result<AdminNotificationHistoryListResponse>> HandleAsync(
        ListTenantNotificationHistoryQuery query,
        CancellationToken cancellationToken)
    {
        PageRequest pageRequest = PageRequest.Normalize(query.Page, query.PageSize);
        AdminNotificationHistoryListResponse response = await repository
            .ListTenantAsync(query.UserId, query.UnreadOnly, pageRequest, cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(response);
    }
}
