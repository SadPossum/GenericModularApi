namespace Catalog.Contracts;

using Shared.Application.Messaging;

public sealed record CatalogItemCreatedIntegrationEvent : IIntegrationEvent
{
    public CatalogItemCreatedIntegrationEvent(
        Guid eventId,
        string tenantId,
        DateTimeOffset occurredAtUtc,
        Guid itemId,
        string sku,
        string name,
        decimal price,
        string currency)
    {
        this.EventId = IntegrationEventContractGuards.RequireId(eventId, nameof(eventId));
        this.TenantId = IntegrationEventContractGuards.NormalizeTenantId(tenantId, nameof(tenantId));
        this.OccurredAtUtc = IntegrationEventContractGuards.RequireOccurredAtUtc(occurredAtUtc, nameof(occurredAtUtc));
        this.ItemId = IntegrationEventContractGuards.RequireId(itemId, nameof(itemId));
        this.Sku = NormalizeSku(sku);
        this.Name = IntegrationEventContractGuards.NormalizeRequiredText(
            name,
            CatalogContractLimits.NameMaxLength,
            nameof(name));
        this.Price = IntegrationEventContractGuards.RequirePositiveDecimal(
            price,
            CatalogContractLimits.PricePrecision,
            CatalogContractLimits.PriceScale,
            nameof(price));
        this.Currency = NormalizeCurrency(currency);
    }

    public Guid EventId { get; }
    public string TenantId { get; }
    public DateTimeOffset OccurredAtUtc { get; }
    public Guid ItemId { get; }
    public string Sku { get; }
    public string Name { get; }
    public decimal Price { get; }
    public string Currency { get; }
    public string EventName => "item-created";
    public int Version => 1;

    private static string NormalizeSku(string sku) =>
        IntegrationEventContractGuards
            .NormalizeRequiredText(sku, CatalogContractLimits.SkuMaxLength, nameof(sku))
            .ToUpperInvariant();

    private static string NormalizeCurrency(string currency)
    {
        string normalized = IntegrationEventContractGuards
            .NormalizeRequiredText(currency, CatalogContractLimits.CurrencyLength, nameof(currency))
            .ToUpperInvariant();

        return normalized.Length == CatalogContractLimits.CurrencyLength
            ? normalized
            : throw new ArgumentException("currency must be a three-letter code.", nameof(currency));
    }
}
