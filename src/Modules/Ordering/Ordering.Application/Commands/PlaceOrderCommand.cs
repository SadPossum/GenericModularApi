namespace Ordering.Application.Commands;

using Ordering.Contracts;
using Shared.AccessControl;
using Shared.Cqrs;

public sealed record PlaceOrderCommand(
    Guid CatalogItemId,
    int Quantity,
    AccessSubject Subject,
    string RegionCode)
    : ITransactionalCommand<OrderDto>;
