namespace Notifications.Application.Validation;

using Notifications.Application.Commands;
using Shared.Cqrs;

internal sealed class MarkAllNotificationBroadcastsReadCommandValidator
    : ICommandValidator<MarkAllNotificationBroadcastsReadCommand>
{
    public IEnumerable<string> Validate(MarkAllNotificationBroadcastsReadCommand command)
    {
        foreach (string failure in NotificationBroadcastValidation.ValidateTenantId(command.TenantId))
        {
            yield return failure;
        }

        foreach (string failure in NotificationBroadcastValidation.ValidateRecipient(
            command.RecipientKind,
            command.RecipientId))
        {
            yield return failure;
        }
    }
}
