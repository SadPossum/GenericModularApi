namespace Catalog.Persistence.Repositories;

using Catalog.Application.Ports;
using Catalog.Contracts;
using Catalog.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Shared.Pagination;

internal sealed class CatalogItemReadRepository(CatalogDbContext dbContext) : ICatalogItemReadRepository
{
    public async Task<CatalogItemDto?> GetAsync(Guid itemId, CancellationToken cancellationToken)
    {
        CatalogItem? item = await dbContext.CatalogItems
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == itemId, cancellationToken)
            .ConfigureAwait(false);

        return item is null ? null : Map(item);
    }

    public async Task<CatalogItemListResponse> ListAsync(PageRequest pageRequest, CancellationToken cancellationToken)
    {
        List<CatalogItem> items = await dbContext.CatalogItems
            .AsNoTracking()
            .OrderBy(item => item.Sku)
            .Skip(pageRequest.SkipCount)
            .Take(pageRequest.PageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new CatalogItemListResponse(items.Select(Map).ToList(), pageRequest.Page, pageRequest.PageSize);
    }

    private static CatalogItemDto Map(CatalogItem item) =>
        new(
            item.Id,
            item.Sku.Value,
            item.Name.Value,
            item.Price.Value,
            item.Currency.Value,
            MapStatus(item.Status));

    private static CatalogItemStatus MapStatus(CatalogItemState status) =>
        status switch
        {
            CatalogItemState.Active => CatalogItemStatus.Active,
            CatalogItemState.Discontinued => CatalogItemStatus.Discontinued,
            _ => CatalogItemStatus.Unknown
        };
}
