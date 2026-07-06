namespace Notifications.Application.Handlers;

using Notifications.Application.Ports;
using Notifications.Application.Queries;
using Notifications.Application.Visibility;
using Notifications.Contracts;
using Shared.Cqrs;
using Shared.Pagination;
using Shared.Results;

internal sealed class ListNotificationHistoryQueryHandler(
    INotificationHistoryRepository repository)
    : IQueryHandler<ListNotificationHistoryQuery, NotificationHistoryListResponse>
{
    public async Task<Result<NotificationHistoryListResponse>> HandleAsync(
        ListNotificationHistoryQuery query,
        CancellationToken cancellationToken)
    {
        if (!NotificationHistoryAccess.CanAccessUserHistory(query.Subject, query.Subject.TenantId))
        {
            return Result.Failure<NotificationHistoryListResponse>(NotificationsApplicationErrors.AccessDenied);
        }

        PageRequest pageRequest = PageRequest.Normalize(query.Page, query.PageSize);
        NotificationHistoryListResponse response = await repository
            .ListAsync(query.Subject, query.UnreadOnly, pageRequest, cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(response);
    }
}
