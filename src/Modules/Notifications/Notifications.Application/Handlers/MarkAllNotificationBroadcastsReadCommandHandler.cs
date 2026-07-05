namespace Notifications.Application.Handlers;

using Notifications.Application.Commands;
using Notifications.Application.Ports;
using Notifications.Contracts;
using Shared.Cqrs;
using Shared.Results;
using Shared.Runtime.Time;

internal sealed class MarkAllNotificationBroadcastsReadCommandHandler(
    INotificationBroadcastRepository repository,
    ISystemClock clock)
    : ICommandHandler<MarkAllNotificationBroadcastsReadCommand, MarkAllNotificationBroadcastsReadResponse>
{
    public async Task<Result<MarkAllNotificationBroadcastsReadResponse>> HandleAsync(
        MarkAllNotificationBroadcastsReadCommand command,
        CancellationToken cancellationToken)
    {
        Result<NotificationBroadcastRecipientContext> recipient =
            NotificationBroadcastRecipientContext.Create(command.TenantId, command.RecipientKind, command.RecipientId);
        if (recipient.IsFailure)
        {
            return Result.Failure<MarkAllNotificationBroadcastsReadResponse>(recipient.Error);
        }

        int updatedCount = await repository
            .MarkAllVisibleReadAsync(
                recipient.Value,
                clock.UtcNow,
                cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(new MarkAllNotificationBroadcastsReadResponse(updatedCount));
    }
}
