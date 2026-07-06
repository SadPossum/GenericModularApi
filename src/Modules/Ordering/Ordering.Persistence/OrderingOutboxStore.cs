namespace Ordering.Persistence;

using Microsoft.Extensions.Options;
using Shared.Messaging.Infrastructure;

internal sealed class OrderingOutboxStore(OrderingDbContext dbContext, IOptions<OutboxOptions> options)
    : EfOutboxStore<OrderingDbContext>(dbContext, options, OrderingMigrations.Schema);
