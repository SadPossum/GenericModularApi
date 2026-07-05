namespace Ordering.Application.Handlers;

using Ordering.Application.Ports;
using Ordering.Application.Queries;
using Ordering.Contracts;
using Shared.Cqrs;
using Shared.Pagination;
using Shared.Results;

internal sealed class ListOrdersQueryHandler(IOrderReadRepository repository)
    : IQueryHandler<ListOrdersQuery, OrderListResponse>
{
    public async Task<Result<OrderListResponse>> HandleAsync(ListOrdersQuery query, CancellationToken cancellationToken)
    {
        PageRequest pageRequest = PageRequest.Normalize(query.Page, query.PageSize);
        OrderListResponse response = await repository.ListAsync(pageRequest, cancellationToken).ConfigureAwait(false);

        return Result.Success(response);
    }
}
