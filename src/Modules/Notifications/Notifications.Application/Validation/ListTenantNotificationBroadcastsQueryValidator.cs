namespace Notifications.Application.Validation;

using Notifications.Application.Queries;
using Shared.Cqrs;
using Shared.Naming;

internal sealed class ListTenantNotificationBroadcastsQueryValidator
    : IQueryValidator<ListTenantNotificationBroadcastsQuery>
{
    public IEnumerable<string> Validate(ListTenantNotificationBroadcastsQuery query)
    {
        if (!TenantIds.TryNormalize(query.TenantId, out _))
        {
            yield return "Notification broadcast tenant id is invalid.";
        }
    }
}
