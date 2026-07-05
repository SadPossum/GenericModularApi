namespace Notifications.Application.Handlers;

using Notifications.Application.Commands;
using Notifications.Application.Ports;
using Notifications.Domain.Errors;
using Shared.Cqrs;
using Shared.Results;
using Shared.Runtime.Time;

internal sealed class MarkNotificationReadCommandHandler(
    INotificationHistoryRepository repository,
    ISystemClock clock)
    : ICommandHandler<MarkNotificationReadCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(MarkNotificationReadCommand command, CancellationToken cancellationToken)
    {
        bool updated = await repository
            .MarkReadAsync(command.NotificationId, command.UserId, clock.UtcNow, cancellationToken)
            .ConfigureAwait(false);

        return updated
            ? Result.Success(Unit.Value)
            : Result.Failure<Unit>(NotificationsDomainErrors.NotificationNotFound);
    }
}
