namespace Catalog.Application.Queries;

using Catalog.Contracts;
using Shared.Application.Cqrs;
using Shared.Application.Queries;

public sealed record ListCatalogItemsQuery(
    int Page = PageRequest.DefaultPage,
    int PageSize = PageRequest.DefaultPageSize)
    : IQuery<CatalogItemListResponse>;
