namespace Notifications.Application.Validation;

using Notifications.Application.Commands;
using Shared.AccessControl;
using Shared.Cqrs;

internal sealed class MarkNotificationReadCommandValidator : ICommandValidator<MarkNotificationReadCommand>
{
    public IEnumerable<string> Validate(MarkNotificationReadCommand command)
    {
        if (command.NotificationId == Guid.Empty)
        {
            yield return "Notification id is required.";
        }

        if (command.Subject is null || command.Subject.Kind != AccessSubjectKind.User)
        {
            yield return "Notification access subject must be a user.";
        }
    }
}
