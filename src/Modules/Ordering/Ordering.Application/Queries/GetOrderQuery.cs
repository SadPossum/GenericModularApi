namespace Ordering.Application.Queries;

using Ordering.Contracts;
using Shared.Cqrs;

public sealed record GetOrderQuery(Guid OrderId) : IQuery<OrderDto>;
