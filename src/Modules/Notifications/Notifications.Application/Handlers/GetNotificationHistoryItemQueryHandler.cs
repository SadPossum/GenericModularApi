namespace Notifications.Application.Handlers;

using Notifications.Application.Ports;
using Notifications.Application.Queries;
using Notifications.Contracts;
using Notifications.Domain.Errors;
using Shared.Cqrs;
using Shared.Results;

internal sealed class GetNotificationHistoryItemQueryHandler(INotificationHistoryRepository repository)
    : IQueryHandler<GetNotificationHistoryItemQuery, NotificationHistoryItem>
{
    public async Task<Result<NotificationHistoryItem>> HandleAsync(
        GetNotificationHistoryItemQuery query,
        CancellationToken cancellationToken)
    {
        NotificationHistoryItem? notification = await repository
            .GetAsync(query.NotificationId, query.UserId, cancellationToken)
            .ConfigureAwait(false);

        return notification is null
            ? Result.Failure<NotificationHistoryItem>(NotificationsDomainErrors.NotificationNotFound)
            : Result.Success(notification);
    }
}
