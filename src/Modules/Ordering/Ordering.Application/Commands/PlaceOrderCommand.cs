namespace Ordering.Application.Commands;

using Ordering.Contracts;
using Shared.Application.Cqrs;

public sealed record PlaceOrderCommand(Guid CatalogItemId, int Quantity) : ITransactionalCommand<OrderDto>;
