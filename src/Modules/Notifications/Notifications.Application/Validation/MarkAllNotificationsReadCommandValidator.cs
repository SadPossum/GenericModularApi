namespace Notifications.Application.Validation;

using Notifications.Application.Commands;
using Shared.AccessControl;
using Shared.Cqrs;

internal sealed class MarkAllNotificationsReadCommandValidator : ICommandValidator<MarkAllNotificationsReadCommand>
{
    public IEnumerable<string> Validate(MarkAllNotificationsReadCommand command)
    {
        if (command.Subject is null || command.Subject.Kind != AccessSubjectKind.User)
        {
            yield return "Notification access subject must be a user.";
        }
    }
}
