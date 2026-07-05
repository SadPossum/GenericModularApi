namespace Shared.Notifications.SignalR;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Shared.Notifications;
using Shared.Runtime;

internal sealed class SignalRUserNotificationSink(
    IHubContext<UserNotificationsHub> hubContext,
    IOptions<ApplicationIdentityOptions> applicationIdentity,
    IOptions<NotificationSignalROptions> options) : IUserNotificationSink
{
    public string ProviderName => "signalr";

    public async ValueTask DeliverAsync(UserNotificationMessage message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        cancellationToken.ThrowIfCancellationRequested();

        if (!options.Value.Enabled)
        {
            return;
        }

        string groupName = NotificationSignalRGroupNames.ForUser(
            applicationIdentity.Value.EffectiveNamespace,
            message.TenantId,
            message.UserId);
        await hubContext.Clients
            .Group(groupName)
            .SendAsync(options.Value.ClientMethodName, message, cancellationToken)
            .ConfigureAwait(false);
    }
}
