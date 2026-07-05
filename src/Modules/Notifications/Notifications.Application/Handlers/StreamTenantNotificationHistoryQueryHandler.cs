namespace Notifications.Application.Handlers;

using Notifications.Application.Ports;
using Notifications.Application.Queries;
using Notifications.Contracts;
using Shared.Cqrs;
using Shared.Results;

internal sealed class StreamTenantNotificationHistoryQueryHandler(INotificationHistoryRepository repository)
    : IQueryHandler<StreamTenantNotificationHistoryQuery, IReadOnlyList<AdminNotificationHistoryItem>>
{
    public async Task<Result<IReadOnlyList<AdminNotificationHistoryItem>>> HandleAsync(
        StreamTenantNotificationHistoryQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<AdminNotificationHistoryItem> items = await repository
            .ListNewForTenantAsync(
                query.UserId,
                query.AfterStreamSequence,
                query.BatchSize,
                cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(items);
    }
}
