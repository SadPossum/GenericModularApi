namespace Ordering.Persistence.Repositories;

using Ordering.Application.Ports;
using Ordering.Domain.Aggregates;

internal sealed class OrderRepository(OrderingDbContext dbContext) : IOrderRepository
{
    public async Task AddAsync(Order order, CancellationToken cancellationToken)
    {
        await dbContext.Orders.AddAsync(order, cancellationToken).ConfigureAwait(false);
    }
}
