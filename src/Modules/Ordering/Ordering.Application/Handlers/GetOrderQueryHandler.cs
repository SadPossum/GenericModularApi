namespace Ordering.Application.Handlers;

using Ordering.Application.Ports;
using Ordering.Application.Queries;
using Ordering.Contracts;
using Shared.Cqrs;
using Shared.Results;

internal sealed class GetOrderQueryHandler(IOrderReadRepository repository)
    : IQueryHandler<GetOrderQuery, OrderDto>
{
    public async Task<Result<OrderDto>> HandleAsync(GetOrderQuery query, CancellationToken cancellationToken)
    {
        OrderDto? order = await repository.GetAsync(query.OrderId, cancellationToken).ConfigureAwait(false);

        return order is null
            ? Result.Failure<OrderDto>(OrderingApplicationErrors.OrderNotFound)
            : Result.Success(order);
    }
}
