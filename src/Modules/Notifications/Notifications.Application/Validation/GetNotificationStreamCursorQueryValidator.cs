namespace Notifications.Application.Validation;

using Notifications.Application.Queries;
using Shared.AccessControl;
using Shared.Cqrs;

internal sealed class GetNotificationStreamCursorQueryValidator : IQueryValidator<GetNotificationStreamCursorQuery>
{
    public IEnumerable<string> Validate(GetNotificationStreamCursorQuery query)
    {
        if (query.Subject is null || query.Subject.Kind != AccessSubjectKind.User)
        {
            yield return "Notification access subject must be a user.";
        }
    }
}
