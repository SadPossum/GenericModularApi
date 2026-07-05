namespace Ordering.Application.Handlers;

using Catalog.Contracts;
using Ordering.Application.Commands;
using Ordering.Application.Ports;
using Ordering.Contracts;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Errors;
using Shared.Cqrs;
using Shared.Runtime.Identity;
using Shared.Tenancy;
using Shared.Runtime.Time;
using Shared.Results;

internal sealed class PlaceOrderCommandHandler(
    IOrderRepository orderRepository,
    ICatalogItemProjectionRepository catalogProjectionRepository,
    ITenantContext tenantContext,
    IIdGenerator idGenerator,
    ISystemClock clock)
    : ICommandHandler<PlaceOrderCommand, OrderDto>
{
    public async Task<Result<OrderDto>> HandleAsync(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tenantContext.TenantId))
        {
            return Result.Failure<OrderDto>(OrderingDomainErrors.TenantRequired);
        }

        CatalogItemProjectionSnapshot? catalogItem = await catalogProjectionRepository
            .GetAsync(command.CatalogItemId, cancellationToken)
            .ConfigureAwait(false);

        if (catalogItem is null)
        {
            return Result.Failure<OrderDto>(OrderingDomainErrors.CatalogItemUnknown);
        }

        if (catalogItem.Status == CatalogItemStatus.Discontinued)
        {
            return Result.Failure<OrderDto>(OrderingDomainErrors.CatalogItemDiscontinued);
        }

        if (catalogItem.Status != CatalogItemStatus.Active)
        {
            return Result.Failure<OrderDto>(OrderingDomainErrors.CatalogItemStatusUnknown);
        }

        Result<Order> orderResult = Order.Create(
            idGenerator.NewId(),
            tenantContext.TenantId,
            catalogItem.CatalogItemId,
            catalogItem.Sku,
            catalogItem.Name,
            catalogItem.Price,
            catalogItem.Currency,
            command.Quantity,
            clock.UtcNow);

        if (orderResult.IsFailure)
        {
            return Result.Failure<OrderDto>(orderResult.Error);
        }

        Order order = orderResult.Value;
        await orderRepository.AddAsync(order, cancellationToken).ConfigureAwait(false);

        return Result.Success(Map(order));
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
            OrderStatus.Submitted,
            order.CreatedAtUtc);
}
