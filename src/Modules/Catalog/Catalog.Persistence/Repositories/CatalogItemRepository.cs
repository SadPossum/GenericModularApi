namespace Catalog.Persistence.Repositories;

using Catalog.Application.Ports;
using Catalog.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

internal sealed class CatalogItemRepository(CatalogDbContext dbContext) : ICatalogItemRepository
{
    public async Task AddAsync(CatalogItem item, CancellationToken cancellationToken)
    {
        await dbContext.CatalogItems.AddAsync(item, cancellationToken).ConfigureAwait(false);
    }

    public async Task<CatalogItem?> GetAsync(Guid itemId, CancellationToken cancellationToken) =>
        await dbContext.CatalogItems
            .FirstOrDefaultAsync(item => item.Id == itemId, cancellationToken)
            .ConfigureAwait(false);

    public async Task<bool> SkuExistsAsync(string sku, Guid? excludingItemId, CancellationToken cancellationToken) =>
        await dbContext.CatalogItems
            .AnyAsync(
                item => item.Sku == sku && (excludingItemId == null || item.Id != excludingItemId.Value),
                cancellationToken)
            .ConfigureAwait(false);
}
