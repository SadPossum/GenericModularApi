namespace Notifications.Persistence;

using Shared.Messaging.Infrastructure;
using Shared.Runtime.Identity;
using Shared.Runtime.Time;

internal sealed class NotificationsInboxStore(
    NotificationsDbContext dbContext,
    ISystemClock clock,
    IIdGenerator idGenerator)
    : EfInboxStore<NotificationsDbContext>(dbContext, clock, idGenerator, NotificationsMigrations.Schema);
