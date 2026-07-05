namespace Notifications.Application.Validation;

using Notifications.Application.Commands;
using Notifications.Contracts;
using Shared.Cqrs;

internal sealed class MarkAllNotificationsReadCommandValidator : ICommandValidator<MarkAllNotificationsReadCommand>
{
    public IEnumerable<string> Validate(MarkAllNotificationsReadCommand command)
    {
        if (!NotificationRecipientUserIds.TryNormalize(command.UserId, out _))
        {
            yield return "Notification user id is required.";
        }
    }
}
