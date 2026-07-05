namespace Notifications.Application.Validation;

using Notifications.Application.Queries;
using Shared.Cqrs;

internal sealed class ListNotificationBroadcastsQueryValidator : IQueryValidator<ListNotificationBroadcastsQuery>
{
    public IEnumerable<string> Validate(ListNotificationBroadcastsQuery query)
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
