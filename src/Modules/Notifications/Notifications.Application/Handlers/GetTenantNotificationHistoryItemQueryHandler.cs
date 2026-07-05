namespace Notifications.Application.Handlers;

using Notifications.Application.Ports;
using Notifications.Application.Queries;
using Notifications.Contracts;
using Shared.Cqrs;
using Shared.Results;

internal sealed class GetTenantNotificationHistoryItemQueryHandler(INotificationHistoryRepository repository)
    : IQueryHandler<GetTenantNotificationHistoryItemQuery, AdminNotificationHistoryItem>
{
    public async Task<Result<AdminNotificationHistoryItem>> HandleAsync(
        GetTenantNotificationHistoryItemQuery query,
        CancellationToken cancellationToken)
    {
        AdminNotificationHistoryItem? notification = await repository
            .GetTenantAsync(query.NotificationId, cancellationToken)
            .ConfigureAwait(false);

        return notification is null
            ? Result.Failure<AdminNotificationHistoryItem>(NotificationsApplicationErrors.NotificationNotFound)
            : Result.Success(notification);
    }
}
