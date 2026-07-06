namespace Shared.Notifications.Cqrs;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shared.Cqrs;
using Shared.Cqrs.Infrastructure;
using Shared.ModuleComposition;
using Shared.Notifications;
using Shared.Notifications.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddUserNotificationsCqrs(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddUserNotificationsInfrastructure();
        builder.AddCqrsInfrastructure();

        if (builder.Services.Any(descriptor => descriptor.ServiceType == typeof(UserNotificationsCqrsRegistrationMarker)))
        {
            return builder;
        }

        builder.Services.AddSingleton<UserNotificationsCqrsRegistrationMarker>();
        builder.ProvideFeature(NotificationsCompositionFeatures.CqrsRequestFlushProvided("Shared.Notifications.Cqrs"));
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Scoped(typeof(ICommandPipelineBehavior<,>), typeof(NotificationRequestCommandBehavior<,>)));
        builder.Services.MoveCommandUnitOfWorkBehaviorToEnd();

        return builder;
    }

    private sealed class UserNotificationsCqrsRegistrationMarker;
}
