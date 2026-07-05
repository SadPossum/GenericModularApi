namespace Notifications.Application.Validation;

using Notifications.Application.Queries;
using Notifications.Contracts;
using Shared.Cqrs;

internal sealed class GetNotificationHistoryItemQueryValidator : IQueryValidator<GetNotificationHistoryItemQuery>
{
    public IEnumerable<string> Validate(GetNotificationHistoryItemQuery query)
    {
        if (query.NotificationId == Guid.Empty)
        {
            yield return "Notification id is required.";
        }

        if (!NotificationRecipientUserIds.TryNormalize(query.UserId, out _))
        {
            yield return "Notification user id is required.";
        }
    }
}
