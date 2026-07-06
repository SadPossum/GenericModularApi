namespace Notifications.Application.Handlers;

using Notifications.Application.Ports;
using Notifications.Application.Queries;
using Notifications.Application.Visibility;
using Notifications.Contracts;
using Shared.Cqrs;
using Shared.Results;

internal sealed class StreamNotificationHistoryQueryHandler(
    INotificationHistoryRepository repository)
    : IQueryHandler<StreamNotificationHistoryQuery, IReadOnlyList<NotificationHistoryItem>>
{
    public async Task<Result<IReadOnlyList<NotificationHistoryItem>>> HandleAsync(
        StreamNotificationHistoryQuery query,
        CancellationToken cancellationToken)
    {
        if (!NotificationHistoryAccess.CanAccessUserHistory(query.Subject, query.Subject.TenantId))
        {
            return Result.Failure<IReadOnlyList<NotificationHistoryItem>>(NotificationsApplicationErrors.AccessDenied);
        }

        IReadOnlyList<NotificationHistoryItem> items = await repository
            .ListNewForUserAsync(
                query.Subject,
                query.AfterStreamSequence,
                query.BatchSize,
                cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(items);
    }
}
