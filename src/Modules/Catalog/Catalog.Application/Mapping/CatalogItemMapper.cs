namespace Catalog.Application.Mapping;

using Catalog.Contracts;
using Catalog.Domain.Aggregates;

internal static class CatalogItemMapper
{
    public static CatalogItemDto ToDto(CatalogItem item) =>
        new(item.Id, item.Sku, item.Name, item.Price, item.Currency, ToContractStatus(item.Status));

    public static CatalogItemStatus ToContractStatus(CatalogItemState status) =>
        status switch
        {
            CatalogItemState.Active => CatalogItemStatus.Active,
            CatalogItemState.Discontinued => CatalogItemStatus.Discontinued,
            _ => CatalogItemStatus.Unknown
        };
}
