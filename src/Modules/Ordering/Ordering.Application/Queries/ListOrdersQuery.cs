namespace Ordering.Application.Queries;

using Ordering.Contracts;
using Shared.AccessControl;
using Shared.Cqrs;

public sealed record ListOrdersQuery(
    AccessSubject Subject,
    int Page = Shared.Pagination.PageRequest.DefaultPage,
    int PageSize = Shared.Pagination.PageRequest.DefaultPageSize)
    : IQuery<OrderListResponse>;
