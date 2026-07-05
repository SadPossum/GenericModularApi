namespace Notifications.Application.Validation;

using Notifications.Application.Queries;
using Shared.Cqrs;

internal sealed class GetTenantNotificationHistoryItemQueryValidator : IQueryValidator<GetTenantNotificationHistoryItemQuery>
{
    public IEnumerable<string> Validate(GetTenantNotificationHistoryItemQuery query)
    {
        if (query.NotificationId == Guid.Empty)
        {
            yield return "Notification id is required.";
        }
    }
}
