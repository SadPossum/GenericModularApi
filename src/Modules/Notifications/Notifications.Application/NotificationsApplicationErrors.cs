namespace Notifications.Application;

using Notifications.Domain.Errors;
using Shared.Results;

public static class NotificationsApplicationErrors
{
    public static readonly Error NotificationNotFound = NotificationsDomainErrors.NotificationNotFound;
    public static readonly Error BroadcastNotFound = NotificationsDomainErrors.BroadcastNotFound;
    public static readonly Error StreamCursorInvalid = new("Notifications.StreamCursorInvalid", "Notification stream cursor is invalid.");
}
