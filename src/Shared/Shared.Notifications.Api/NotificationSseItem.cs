namespace Shared.Notifications.Api;

using Shared.Notifications;

public sealed record NotificationSseItem(
    NotificationSseItemKind Kind,
    DateTimeOffset SentAtUtc,
    UserNotificationMessage? Notification)
{
    public static NotificationSseItem FromNotification(UserNotificationMessage notification) =>
        new(NotificationSseItemKind.Notification, DateTimeOffset.UtcNow, notification);

    public static NotificationSseItem Heartbeat() =>
        new(NotificationSseItemKind.Heartbeat, DateTimeOffset.UtcNow, null);
}
