namespace Ordering.Persistence;

using Shared.Application.Identity;
using Shared.Application.Time;
using Shared.Infrastructure.Messaging;

internal sealed class OrderingInboxStore(OrderingDbContext dbContext, ISystemClock clock, IIdGenerator idGenerator)
    : EfInboxStore<OrderingDbContext>(dbContext, clock, idGenerator, OrderingMigrations.Schema);
