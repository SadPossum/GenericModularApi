namespace Ordering.Application.Queries;

using Ordering.Contracts;
using Shared.AccessControl;
using Shared.Cqrs;

public sealed record GetOrderQuery(Guid OrderId, AccessSubject Subject) : IQuery<OrderDto>;
