namespace Ordering.Persistence;

using Shared.Runtime.Identity;
using Shared.Runtime.Time;
using Shared.Messaging.Infrastructure;

internal sealed class OrderingInboxStore(OrderingDbContext dbContext, ISystemClock clock, IIdGenerator idGenerator)
    : EfInboxStore<OrderingDbContext>(dbContext, clock, idGenerator, OrderingMigrations.Schema);
