namespace Notifications.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Notifications.Application.Ports;
using Notifications.Persistence.Repositories;
using Shared.Cqrs.UnitOfWork;
using Shared.Messaging;
using Shared.Notifications;
using Shared.Persistence.EntityFrameworkCore;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddNotificationsPersistence(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddPersistenceOptions(builder.Configuration);

        builder.Services.TryAddModuleDbContext<NotificationsDbContext>(options =>
            options.UseConfiguredProvider(
                builder.Configuration,
                NotificationsMigrations.SqlServerAssembly,
                NotificationsMigrations.PostgreSqlAssembly,
                NotificationsMigrations.Schema,
                NotificationsMigrations.HistoryTable));

        builder.Services.TryAddScoped<INotificationHistoryRepository, NotificationHistoryRepository>();
        builder.Services.TryAddScoped<INotificationBroadcastRepository, NotificationBroadcastRepository>();
        builder.Services.TryAddEnumerable([
            ServiceDescriptor.Scoped<IUnitOfWork, NotificationsUnitOfWork>(),
            ServiceDescriptor.Scoped<IInboxStore, NotificationsInboxStore>(),
            ServiceDescriptor.Scoped<IUserNotificationHistoryWriter, NotificationHistoryWriter>()
        ]);

        return builder;
    }
}
