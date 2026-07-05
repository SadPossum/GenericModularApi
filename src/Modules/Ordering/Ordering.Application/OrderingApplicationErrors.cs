namespace Ordering.Application;

using Ordering.Domain.Errors;
using Shared.Results;

public static class OrderingApplicationErrors
{
    public static Error OrderNotFound => OrderingDomainErrors.OrderNotFound;
    public static Error CatalogItemUnknown => OrderingDomainErrors.CatalogItemUnknown;
    public static Error CatalogItemDiscontinued => OrderingDomainErrors.CatalogItemDiscontinued;
    public static Error CatalogItemStatusUnknown => OrderingDomainErrors.CatalogItemStatusUnknown;
}
