namespace Notifications.Application.Handlers;

using Notifications.Application.Ports;
using Notifications.Application.Queries;
using Notifications.Contracts;
using Shared.Cqrs;
using Shared.Results;

internal sealed class StreamNotificationHistoryQueryHandler(INotificationHistoryRepository repository)
    : IQueryHandler<StreamNotificationHistoryQuery, IReadOnlyList<NotificationHistoryItem>>
{
    public async Task<Result<IReadOnlyList<NotificationHistoryItem>>> HandleAsync(
        StreamNotificationHistoryQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<NotificationHistoryItem> items = await repository
            .ListNewForUserAsync(
                query.UserId,
                query.AfterStreamSequence,
                query.BatchSize,
                cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(items);
    }
}
