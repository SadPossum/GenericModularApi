namespace Notifications.Persistence;

using Microsoft.EntityFrameworkCore;
using Notifications.Domain.Aggregates;
using Notifications.Domain.Entities;
using Shared.Messaging.Infrastructure;
using Shared.Persistence.EntityFrameworkCore;
using Shared.Tenancy;

public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options, ITenantContext tenantContext)
    : TenantAwareDbContext<NotificationsDbContext>(options, tenantContext)
{
    public DbSet<UserNotification> UserNotifications => this.Set<UserNotification>();
    public DbSet<NotificationBroadcast> NotificationBroadcasts => this.Set<NotificationBroadcast>();
    public DbSet<NotificationBroadcastRead> NotificationBroadcastReads => this.Set<NotificationBroadcastRead>();
    public DbSet<InboxMessage> InboxMessages => this.Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(NotificationsMigrations.Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationsDbContext).Assembly);
        this.ApplyTenantConventions(modelBuilder);
    }
}
