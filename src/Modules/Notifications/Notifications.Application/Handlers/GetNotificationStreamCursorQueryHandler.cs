namespace Notifications.Application.Handlers;

using Notifications.Application.Ports;
using Notifications.Application.Queries;
using Notifications.Application.Visibility;
using Shared.Cqrs;
using Shared.Results;

internal sealed class GetNotificationStreamCursorQueryHandler(
    INotificationHistoryRepository repository)
    : IQueryHandler<GetNotificationStreamCursorQuery, long>
{
    public async Task<Result<long>> HandleAsync(
        GetNotificationStreamCursorQuery query,
        CancellationToken cancellationToken)
    {
        if (!NotificationHistoryAccess.CanAccessUserHistory(query.Subject, query.Subject.TenantId))
        {
            return Result.Failure<long>(NotificationsApplicationErrors.AccessDenied);
        }

        long cursor = await repository
            .GetCurrentStreamSequenceForUserAsync(query.Subject, cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(cursor);
    }
}
