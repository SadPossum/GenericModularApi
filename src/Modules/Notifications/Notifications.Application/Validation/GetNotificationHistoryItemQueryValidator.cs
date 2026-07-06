namespace Notifications.Application.Validation;

using Notifications.Application.Queries;
using Shared.AccessControl;
using Shared.Cqrs;

internal sealed class GetNotificationHistoryItemQueryValidator : IQueryValidator<GetNotificationHistoryItemQuery>
{
    public IEnumerable<string> Validate(GetNotificationHistoryItemQuery query)
    {
        if (query.NotificationId == Guid.Empty)
        {
            yield return "Notification id is required.";
        }

        if (query.Subject is null || query.Subject.Kind != AccessSubjectKind.User)
        {
            yield return "Notification access subject must be a user.";
        }
    }
}
