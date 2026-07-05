namespace Notifications.Persistence.PostgreSqlMigrations;

using Microsoft.EntityFrameworkCore.Design;
using Notifications.Persistence;
using Shared.Persistence.EntityFrameworkCore;

public sealed class NotificationsPostgreSqlDesignTimeDbContextFactory : IDesignTimeDbContextFactory<NotificationsDbContext>
{
    public NotificationsDbContext CreateDbContext(string[] args)
    {
        return new NotificationsDbContext(
            DesignTimeDbContextOptionsFactory.CreatePostgreSqlOptions<NotificationsDbContext>(
                args,
                NotificationsMigrations.PostgreSqlAssembly,
                NotificationsMigrations.Schema,
                NotificationsMigrations.HistoryTable),
            new DesignTimeTenantContext());
    }
}
