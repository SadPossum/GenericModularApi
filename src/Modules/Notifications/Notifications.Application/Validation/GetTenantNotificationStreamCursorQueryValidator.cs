namespace Notifications.Application.Validation;

using Notifications.Application.Queries;
using Notifications.Contracts;
using Shared.Cqrs;

internal sealed class GetTenantNotificationStreamCursorQueryValidator : IQueryValidator<GetTenantNotificationStreamCursorQuery>
{
    public IEnumerable<string> Validate(GetTenantNotificationStreamCursorQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.UserId) &&
            !NotificationRecipientUserIds.TryNormalize(query.UserId, out _))
        {
            yield return "Notification user id is invalid.";
        }
    }
}
