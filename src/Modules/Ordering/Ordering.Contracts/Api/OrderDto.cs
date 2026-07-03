namespace Ordering.Contracts;

public sealed record OrderDto(
    Guid OrderId,
    Guid CatalogItemId,
    string CatalogSku,
    string CatalogItemName,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal Total,
    OrderStatus Status,
    DateTimeOffset CreatedAtUtc);
