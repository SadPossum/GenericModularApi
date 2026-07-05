namespace Shared.Notifications;

public interface IUserNotificationHistoryWriter
{
    ValueTask SaveAsync(UserNotificationMessage message, CancellationToken cancellationToken = default);
}
