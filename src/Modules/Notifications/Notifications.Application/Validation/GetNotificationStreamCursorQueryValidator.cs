namespace Notifications.Application.Validation;

using Notifications.Application.Queries;
using Notifications.Contracts;
using Shared.Cqrs;

internal sealed class GetNotificationStreamCursorQueryValidator : IQueryValidator<GetNotificationStreamCursorQuery>
{
    public IEnumerable<string> Validate(GetNotificationStreamCursorQuery query)
    {
        if (!NotificationRecipientUserIds.TryNormalize(query.UserId, out _))
        {
            yield return "Notification user id is required.";
        }
    }
}
