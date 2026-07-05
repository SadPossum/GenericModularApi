namespace Notifications.Application.Validation;

using Notifications.Contracts;
using Shared.Naming;
using DomainRecipientKind = Notifications.Domain.ValueObjects.NotificationBroadcastRecipientKind;

internal static class NotificationBroadcastValidation
{
    public static IEnumerable<string> ValidateRecipient(NotificationBroadcastRecipientKind recipientKind, string recipientId)
    {
        DomainRecipientKind domainRecipientKind = NotificationBroadcastRecipientKindMapper.ToDomainValue(recipientKind);
        if (domainRecipientKind is not DomainRecipientKind.User and not DomainRecipientKind.Admin)
        {
            yield return "Notification broadcast recipient kind is invalid.";
        }

        if (!NotificationRecipientUserIds.TryNormalize(recipientId, out _))
        {
            yield return "Notification broadcast recipient id is required.";
        }
    }

    public static IEnumerable<string> ValidateTenantId(string? tenantId)
    {
        if (!string.IsNullOrWhiteSpace(tenantId) && !TenantIds.TryNormalize(tenantId, out _))
        {
            yield return "Notification broadcast tenant id is invalid.";
        }
    }
}
