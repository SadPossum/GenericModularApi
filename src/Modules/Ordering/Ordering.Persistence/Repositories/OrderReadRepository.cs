namespace Ordering.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using Ordering.Application.Ports;
using Ordering.Contracts;
using Ordering.Domain.Aggregates;
using Shared.Pagination;

internal sealed class OrderReadRepository(OrderingDbContext dbContext) : IOrderReadRepository
{
    public async Task<OrderDto?> GetAsync(Guid orderId, CancellationToken cancellationToken)
    {
        Order? order = await dbContext.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == orderId, cancellationToken)
            .ConfigureAwait(false);

        return order is null ? null : Map(order);
    }

    public async Task<OrderListResponse> ListAsync(PageRequest pageRequest, CancellationToken cancellationToken)
    {
        IQueryable<Order> orders = dbContext.Orders.AsNoTracking();

        int totalCount = await orders.CountAsync(cancellationToken).ConfigureAwait(false);
        Order[] items = await orders
            .OrderByDescending(order => order.CreatedAtUtc)
            .ThenBy(order => order.Id)
            .Skip(pageRequest.SkipCount)
            .Take(pageRequest.PageSize)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new OrderListResponse(
            items.Select(Map).ToArray(),
            pageRequest.Page,
            pageRequest.PageSize,
            totalCount);
    }

    private static OrderDto Map(Order order) =>
        new(
            order.Id,
            order.CatalogItemId,
            order.CatalogSku,
            order.CatalogItemName,
            order.UnitPrice,
            order.Currency,
            order.Quantity.Value,
            order.Total.Value,
            MapStatus(order.Status),
            order.CreatedAtUtc);

    private static OrderStatus MapStatus(OrderState status) =>
        status switch
        {
            OrderState.Submitted => OrderStatus.Submitted,
            _ => OrderStatus.Unknown
        };
}
