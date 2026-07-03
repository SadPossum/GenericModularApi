namespace Catalog.Domain.Events;

using Catalog.Domain.Aggregates;
using Shared.Domain;

public sealed record CatalogItemUpdatedDomainEvent : IDomainEvent
{
    public CatalogItemUpdatedDomainEvent(
        Guid eventId,
        DateTimeOffset occurredAtUtc,
        Guid itemId,
        string tenantId,
        string sku,
        string name,
        decimal price,
        string currency,
        CatalogItemState status)
    {
        this.EventId = DomainEventGuards.RequireId(eventId, nameof(eventId));
        this.OccurredAtUtc = DomainEventGuards.RequireOccurredAtUtc(occurredAtUtc, nameof(occurredAtUtc));
        this.ItemId = DomainEventGuards.RequireId(itemId, nameof(itemId));
        this.TenantId = DomainEventGuards.NormalizeTenantId(tenantId, nameof(tenantId));
        this.Sku = NormalizeSku(sku);
        this.Name = DomainEventGuards.NormalizeRequiredText(name, CatalogItem.NameMaxLength, nameof(name));
        this.Price = DomainEventGuards.RequirePositiveDecimal(
            price,
            CatalogItem.PricePrecision,
            CatalogItem.PriceScale,
            nameof(price));
        this.Currency = NormalizeCurrency(currency);
        this.Status = DomainEventGuards.NormalizeDefinedOrUnknown(status);
    }

    public Guid EventId { get; }
    public DateTimeOffset OccurredAtUtc { get; }
    public Guid ItemId { get; }
    public string TenantId { get; }
    public string Sku { get; }
    public string Name { get; }
    public decimal Price { get; }
    public string Currency { get; }
    public CatalogItemState Status { get; }

    private static string NormalizeSku(string sku) =>
        DomainEventGuards
            .NormalizeRequiredText(sku, CatalogItem.SkuMaxLength, nameof(sku))
            .ToUpperInvariant();

    private static string NormalizeCurrency(string currency)
    {
        string normalized = DomainEventGuards
            .NormalizeRequiredText(currency, CatalogItem.CurrencyLength, nameof(currency))
            .ToUpperInvariant();

        return normalized.Length == CatalogItem.CurrencyLength
            ? normalized
            : throw new ArgumentException("currency must be a three-letter code.", nameof(currency));
    }
}
