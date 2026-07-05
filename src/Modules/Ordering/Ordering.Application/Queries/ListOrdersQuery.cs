namespace Ordering.Application.Queries;

using Ordering.Contracts;
using Shared.Cqrs;

public sealed record ListOrdersQuery(
    int Page = Shared.Pagination.PageRequest.DefaultPage,
    int PageSize = Shared.Pagination.PageRequest.DefaultPageSize)
    : IQuery<OrderListResponse>;
