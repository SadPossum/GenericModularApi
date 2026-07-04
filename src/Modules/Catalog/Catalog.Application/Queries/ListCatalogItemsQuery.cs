namespace Catalog.Application.Queries;

using Catalog.Contracts;
using Shared.Cqrs;
using Shared.Pagination;

public sealed record ListCatalogItemsQuery(
    int Page = PageRequest.DefaultPage,
    int PageSize = PageRequest.DefaultPageSize)
    : IQuery<CatalogItemListResponse>;
