namespace Catalog.Contracts;

using Shared.Application.Messaging;

public sealed record CatalogItemCreatedIntegrationEvent : IntegrationEvent
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
        : base(eventId, tenantId, occurredAtUtc, "item-created", version: 1)
    {
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

    public Guid ItemId { get; }
    public string Sku { get; }
    public string Name { get; }
    public decimal Price { get; }
    public string Currency { get; }

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
