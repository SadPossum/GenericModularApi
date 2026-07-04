namespace Catalog.Contracts;

using Shared.Messaging;
using Shared.Tenancy;

[IntegrationEventName(CatalogItemDiscontinuedIntegrationEvent.EventType)]
[IntegrationEventVersion(CatalogItemDiscontinuedIntegrationEvent.EventVersion)]
[TenantScoped]
public sealed record CatalogItemDiscontinuedIntegrationEvent : IntegrationEvent
{
    public const string EventType = "item-discontinued";
    public const int EventVersion = 1;

    public CatalogItemDiscontinuedIntegrationEvent(
        Guid eventId,
        string tenantId,
        DateTimeOffset occurredAtUtc,
        Guid itemId,
        string sku)
        : base(eventId, tenantId, occurredAtUtc, EventType, EventVersion)
    {
        this.ItemId = IntegrationEventContractGuards.RequireId(itemId, nameof(itemId));
        this.Sku = IntegrationEventContractGuards
            .NormalizeRequiredText(sku, CatalogContractLimits.SkuMaxLength, nameof(sku))
            .ToUpperInvariant();
    }

    public Guid ItemId { get; }
    public string Sku { get; }
}
