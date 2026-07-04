namespace Catalog.Persistence.Repositories;

using Catalog.Contracts;
using Catalog.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Shared.ProjectionRebuild;

internal sealed class CatalogItemProjectionExportSource(CatalogDbContext dbContext) : ICatalogItemProjectionExportSource
{
    public async Task<ProjectionReadBatch<CatalogItemProjectionExport>> ReadAsync(
        ProjectionRebuildRequest request,
        string? cursor,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        string? normalizedCursor = NormalizeCursor(cursor);
        IQueryable<CatalogItem> query = dbContext.CatalogItems.AsNoTracking();
        if (normalizedCursor is not null)
        {
#pragma warning disable CA1309
            query = query.Where(item => string.Compare(item.Sku, normalizedCursor) > 0);
#pragma warning restore CA1309
        }

        List<CatalogItem> rows = await query
            .OrderBy(item => item.Sku)
            .Take(request.BatchSize + 1)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        bool hasMore = rows.Count > request.BatchSize;
        CatalogItem[] page = rows.Take(request.BatchSize).ToArray();
        string? nextCursor = page.Length == 0 ? null : page[^1].Sku;

        return new ProjectionReadBatch<CatalogItemProjectionExport>(
            page.Select(Map).ToArray(),
            nextCursor,
            hasMore);
    }

    private static CatalogItemProjectionExport Map(CatalogItem item) =>
        new(
            item.TenantId,
            item.Id,
            item.Sku,
            item.Name,
            item.Price,
            item.Currency,
            MapStatus(item.Status));

    private static CatalogItemStatus MapStatus(CatalogItemState status) =>
        status switch
        {
            CatalogItemState.Active => CatalogItemStatus.Active,
            CatalogItemState.Discontinued => CatalogItemStatus.Discontinued,
            _ => CatalogItemStatus.Unknown
        };

    private static string? NormalizeCursor(string? cursor) =>
        string.IsNullOrWhiteSpace(cursor)
            ? null
            : CatalogItem.NormalizeSku(cursor);
}
