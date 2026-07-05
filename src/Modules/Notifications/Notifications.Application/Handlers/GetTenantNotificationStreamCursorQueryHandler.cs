namespace Notifications.Application.Handlers;

using Notifications.Application.Ports;
using Notifications.Application.Queries;
using Shared.Cqrs;
using Shared.Results;

internal sealed class GetTenantNotificationStreamCursorQueryHandler(INotificationHistoryRepository repository)
    : IQueryHandler<GetTenantNotificationStreamCursorQuery, long>
{
    public async Task<Result<long>> HandleAsync(
        GetTenantNotificationStreamCursorQuery query,
        CancellationToken cancellationToken)
    {
        long cursor = await repository
            .GetCurrentStreamSequenceForTenantAsync(query.UserId, cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(cursor);
    }
}
