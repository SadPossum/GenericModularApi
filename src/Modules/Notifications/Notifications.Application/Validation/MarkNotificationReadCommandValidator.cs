namespace Notifications.Application.Validation;

using Notifications.Application.Commands;
using Notifications.Contracts;
using Shared.Cqrs;

internal sealed class MarkNotificationReadCommandValidator : ICommandValidator<MarkNotificationReadCommand>
{
    public IEnumerable<string> Validate(MarkNotificationReadCommand command)
    {
        if (command.NotificationId == Guid.Empty)
        {
            yield return "Notification id is required.";
        }

        if (!NotificationRecipientUserIds.TryNormalize(command.UserId, out _))
        {
            yield return "Notification user id is required.";
        }
    }
}
