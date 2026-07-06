namespace Notifications.Application.Handlers;

using Notifications.Application.Commands;
using Notifications.Application.Ports;
using Notifications.Application.Visibility;
using Notifications.Contracts;
using Shared.Cqrs;
using Shared.Results;
using Shared.Runtime.Time;

internal sealed class MarkAllNotificationsReadCommandHandler(
    INotificationHistoryRepository repository,
    ISystemClock clock)
    : ICommandHandler<MarkAllNotificationsReadCommand, MarkAllNotificationsReadResponse>
{
    public async Task<Result<MarkAllNotificationsReadResponse>> HandleAsync(
        MarkAllNotificationsReadCommand command,
        CancellationToken cancellationToken)
    {
        if (!NotificationHistoryAccess.CanAccessUserHistory(command.Subject, command.Subject.TenantId))
        {
            return Result.Failure<MarkAllNotificationsReadResponse>(NotificationsApplicationErrors.AccessDenied);
        }

        int updatedCount = await repository
            .MarkAllReadAsync(command.Subject, clock.UtcNow, cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(new MarkAllNotificationsReadResponse(updatedCount));
    }
}
