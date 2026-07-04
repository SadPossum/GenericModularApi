namespace Catalog.Application.Handlers;

using Catalog.Application.Ports;
using Catalog.Application.Queries;
using Catalog.Contracts;
using Shared.Caching;
using Shared.Cqrs;
using Shared.Pagination;
using Shared.Results;

internal sealed class ListCatalogItemsQueryHandler(
    ICatalogItemReadRepository repository,
    IApplicationCache cache)
    : IQueryHandler<ListCatalogItemsQuery, CatalogItemListResponse>
{
    public async Task<Result<CatalogItemListResponse>> HandleAsync(
        ListCatalogItemsQuery query,
        CancellationToken cancellationToken)
    {
        PageRequest pageRequest = PageRequest.Normalize(query.Page, query.PageSize);

        CatalogItemListResponse response = await cache.GetOrCreateAsync(
            CatalogCache.Items(pageRequest.Page, pageRequest.PageSize),
            token => new ValueTask<CatalogItemListResponse>(repository.ListAsync(pageRequest, token)),
            tags: [CatalogCache.ItemsTag()],
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return Result.Success(response);
    }
}
