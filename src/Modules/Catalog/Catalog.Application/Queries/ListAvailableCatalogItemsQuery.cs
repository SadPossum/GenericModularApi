namespace Catalog.Application.Queries;

using Catalog.Contracts;
using Shared.AccessControl;
using Shared.Cqrs;
using Shared.Pagination;

public sealed record ListAvailableCatalogItemsQuery(
    AccessSubject Subject,
    string RegionCode,
    string? SubjectRegionCode,
    int Page = PageRequest.DefaultPage,
    int PageSize = PageRequest.DefaultPageSize)
    : IQuery<CatalogItemListResponse>;
