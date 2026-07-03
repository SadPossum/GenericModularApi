namespace Ordering.Persistence;

using Shared.Application.Events;
using Shared.Infrastructure.Persistence;

internal sealed class OrderingUnitOfWork(OrderingDbContext dbContext, IDomainEventDispatcher domainEventDispatcher)
    : EfDomainEventUnitOfWork<OrderingDbContext>(OrderingMigrations.Schema, dbContext, domainEventDispatcher)
{
}
