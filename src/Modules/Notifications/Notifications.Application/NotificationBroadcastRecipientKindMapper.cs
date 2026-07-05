namespace Notifications.Application;

using ContractRecipientKind = Notifications.Contracts.NotificationBroadcastRecipientKind;
using DomainRecipientKind = Notifications.Domain.ValueObjects.NotificationBroadcastRecipientKind;

internal static class NotificationBroadcastRecipientKindMapper
{
    public static DomainRecipientKind ToDomainValue(ContractRecipientKind recipientKind) =>
        recipientKind switch
        {
            ContractRecipientKind.User => DomainRecipientKind.User,
            ContractRecipientKind.Admin => DomainRecipientKind.Admin,
            _ => DomainRecipientKind.Unknown
        };
}
