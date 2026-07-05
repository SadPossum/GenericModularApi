namespace Notifications.Application.Handlers;

using Notifications.Application.Queries;
using Notifications.Application.Ports;
using Notifications.Contracts;
using Shared.Cqrs;
using Shared.Results;

internal sealed class StreamNotificationBroadcastsQueryHandler(INotificationBroadcastRepository repository)
    : IQueryHandler<StreamNotificationBroadcastsQuery, IReadOnlyList<NotificationBroadcastItem>>
{
    public async Task<Result<IReadOnlyList<NotificationBroadcastItem>>> HandleAsync(
        StreamNotificationBroadcastsQuery query,
        CancellationToken cancellationToken)
    {
        Result<NotificationBroadcastRecipientContext> recipient =
            NotificationBroadcastRecipientContext.Create(query.TenantId, query.RecipientKind, query.RecipientId);
        if (recipient.IsFailure)
        {
            return Result.Failure<IReadOnlyList<NotificationBroadcastItem>>(recipient.Error);
        }

        IReadOnlyList<NotificationBroadcastItem> broadcasts = await repository
            .ListNewVisibleAsync(
                recipient.Value,
                query.AfterStreamSequence,
                query.BatchSize,
                cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(broadcasts);
    }
}
