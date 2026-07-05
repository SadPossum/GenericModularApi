namespace Shared.Notifications;

public interface IUserNotificationSubscription : IAsyncDisposable
{
    UserNotificationTarget Target { get; }

    IAsyncEnumerable<UserNotificationMessage> ReadAllAsync(CancellationToken cancellationToken = default);
}
