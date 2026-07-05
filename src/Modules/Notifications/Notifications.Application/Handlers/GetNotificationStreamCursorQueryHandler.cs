namespace Notifications.Application.Handlers;

using Notifications.Application.Ports;
using Notifications.Application.Queries;
using Shared.Cqrs;
using Shared.Results;

internal sealed class GetNotificationStreamCursorQueryHandler(INotificationHistoryRepository repository)
    : IQueryHandler<GetNotificationStreamCursorQuery, long>
{
    public async Task<Result<long>> HandleAsync(
        GetNotificationStreamCursorQuery query,
        CancellationToken cancellationToken)
    {
        long cursor = await repository
            .GetCurrentStreamSequenceForUserAsync(query.UserId, cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(cursor);
    }
}
