namespace Notifications.Application.Handlers;

using Notifications.Application.Queries;
using Notifications.Application.Ports;
using Notifications.Contracts;
using Shared.Cqrs;
using Shared.Results;

internal sealed class GetNotificationBroadcastQueryHandler(INotificationBroadcastRepository repository)
    : IQueryHandler<GetNotificationBroadcastQuery, NotificationBroadcastItem>
{
    public async Task<Result<NotificationBroadcastItem>> HandleAsync(
        GetNotificationBroadcastQuery query,
        CancellationToken cancellationToken)
    {
        Result<NotificationBroadcastRecipientContext> recipient =
            NotificationBroadcastRecipientContext.Create(query.TenantId, query.RecipientKind, query.RecipientId);
        if (recipient.IsFailure)
        {
            return Result.Failure<NotificationBroadcastItem>(recipient.Error);
        }

        NotificationBroadcastItem? broadcast = await repository
            .GetVisibleAsync(query.BroadcastId, recipient.Value, cancellationToken)
            .ConfigureAwait(false);

        return broadcast is null
            ? Result.Failure<NotificationBroadcastItem>(NotificationsApplicationErrors.BroadcastNotFound)
            : Result.Success(broadcast);
    }
}
