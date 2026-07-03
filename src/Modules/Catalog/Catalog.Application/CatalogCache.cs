namespace Catalog.Application;

using System.Globalization;
using Catalog.Contracts;
using Shared.Application.Caching;

internal static class CatalogCache
{
    public static CacheKey Item(Guid itemId) =>
        CacheKey.Tenant(CatalogModuleMetadata.Name, CatalogModuleMetadata.ItemCacheEntry, itemId.ToString("N"));

    public static CacheKey Items(int page, int pageSize) =>
        CacheKey.Tenant(
            CatalogModuleMetadata.Name,
            CatalogModuleMetadata.ItemsCacheEntry,
            page.ToString(CultureInfo.InvariantCulture),
            pageSize.ToString(CultureInfo.InvariantCulture));

    public static CacheTag ItemsTag() =>
        CacheTag.Tenant(CatalogModuleMetadata.Name, CatalogModuleMetadata.ItemsCacheTag);
}
