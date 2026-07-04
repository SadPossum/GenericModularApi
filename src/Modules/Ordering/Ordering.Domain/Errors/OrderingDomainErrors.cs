namespace Ordering.Domain.Errors;

using Shared.Results;

public static class OrderingDomainErrors
{
    public static readonly Error OrderIdRequired = new("Ordering.OrderIdRequired", "An order id is required.");
    public static readonly Error TenantRequired = new("Ordering.TenantRequired", "A tenant id is required.");
    public static readonly Error TenantInvalid = new("Ordering.TenantInvalid", "The tenant id is not valid.");
    public static readonly Error CatalogItemRequired = new("Ordering.CatalogItemRequired", "A catalog item id is required.");
    public static readonly Error CatalogSkuRequired = new("Ordering.CatalogSkuRequired", "A catalog item SKU is required.");
    public static readonly Error CatalogSkuTooLong = new("Ordering.CatalogSkuTooLong", "The catalog item SKU is too long.");
    public static readonly Error CatalogItemNameRequired = new("Ordering.CatalogItemNameRequired", "A catalog item name is required.");
    public static readonly Error CatalogItemNameTooLong = new("Ordering.CatalogItemNameTooLong", "The catalog item name is too long.");
    public static readonly Error CatalogItemPriceMustBePositive = new("Ordering.CatalogItemPriceMustBePositive", "The catalog item price must be greater than zero.");
    public static readonly Error CatalogItemPriceNotSupported = new("Ordering.CatalogItemPriceNotSupported", "The catalog item price must fit the configured decimal precision.");
    public static readonly Error CatalogItemCurrencyInvalid = new("Ordering.CatalogItemCurrencyInvalid", "The catalog item currency must be a three-letter code.");
    public static readonly Error QuantityMustBePositive = new("Ordering.QuantityMustBePositive", "The order quantity must be greater than zero.");
    public static readonly Error OrderTotalNotSupported = new("Ordering.OrderTotalNotSupported", "The order total must fit the configured decimal precision.");
    public static readonly Error CatalogItemUnknown = new("Ordering.CatalogItemUnknown", "The catalog item is unknown to Ordering.");
    public static readonly Error CatalogItemDiscontinued = new("Ordering.CatalogItemDiscontinued", "The catalog item is discontinued.");
    public static readonly Error CatalogItemStatusUnknown = new("Ordering.CatalogItemStatusUnknown", "The catalog item status is unknown to Ordering.");
}
