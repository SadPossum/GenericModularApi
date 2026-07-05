namespace Notifications.Application.Validation;

using Notifications.Application.Queries;
using Shared.Cqrs;

internal sealed class GetNotificationBroadcastStreamCursorQueryValidator
    : IQueryValidator<GetNotificationBroadcastStreamCursorQuery>
{
    public IEnumerable<string> Validate(GetNotificationBroadcastStreamCursorQuery query)
    {
        foreach (string failure in NotificationBroadcastValidation.ValidateTenantId(query.TenantId))
        {
            yield return failure;
        }

        foreach (string failure in NotificationBroadcastValidation.ValidateRecipient(
            query.RecipientKind,
            query.RecipientId))
        {
            yield return failure;
        }
    }
}
