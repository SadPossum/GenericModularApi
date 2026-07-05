namespace Notifications.Application.Handlers;

using Notifications.Application.Commands;
using Notifications.Application.Ports;
using Shared.Cqrs;
using Shared.Results;
using Shared.Runtime.Time;

internal sealed class MarkNotificationBroadcastReadCommandHandler(
    INotificationBroadcastRepository repository,
    ISystemClock clock)
    : ICommandHandler<MarkNotificationBroadcastReadCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(
        MarkNotificationBroadcastReadCommand command,
        CancellationToken cancellationToken)
    {
        Result<NotificationBroadcastRecipientContext> recipient =
            NotificationBroadcastRecipientContext.Create(command.TenantId, command.RecipientKind, command.RecipientId);
        if (recipient.IsFailure)
        {
            return Result.Failure<Unit>(recipient.Error);
        }

        bool updated = await repository
            .MarkReadAsync(
                command.BroadcastId,
                recipient.Value,
                clock.UtcNow,
                cancellationToken)
            .ConfigureAwait(false);

        return updated
            ? Result.Success(Unit.Value)
            : Result.Failure<Unit>(NotificationsApplicationErrors.BroadcastNotFound);
    }
}
