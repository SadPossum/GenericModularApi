namespace Ordering.Application.Commands;

using Ordering.Contracts;
using Shared.Cqrs;

public sealed record PlaceOrderCommand(Guid CatalogItemId, int Quantity) : ITransactionalCommand<OrderDto>;
