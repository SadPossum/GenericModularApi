namespace Shared.Notifications;

public interface IUserNotificationFeed
{
    IUserNotificationSubscription Subscribe(
        UserNotificationTarget target,
        CancellationToken cancellationToken = default);
}
