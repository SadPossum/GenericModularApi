namespace Catalog.Contracts;

using Shared.Messaging;

public sealed record CatalogItemDiscontinuedIntegrationEvent : IntegrationEvent
{
    public CatalogItemDiscontinuedIntegrationEvent(
        Guid eventId,
        string tenantId,
        DateTimeOffset occurredAtUtc,
        Guid itemId,
        string sku)
        : base(eventId, tenantId, occurredAtUtc, "item-discontinued", version: 1)
    {
        this.ItemId = IntegrationEventContractGuards.RequireId(itemId, nameof(itemId));
        this.Sku = IntegrationEventContractGuards
            .NormalizeRequiredText(sku, CatalogContractLimits.SkuMaxLength, nameof(sku))
            .ToUpperInvariant();
    }

    public Guid ItemId { get; }
    public string Sku { get; }
}
