namespace Catalog.Contracts;

using Shared.Application.Messaging;

public sealed record CatalogItemDiscontinuedIntegrationEvent : IIntegrationEvent
{
    public CatalogItemDiscontinuedIntegrationEvent(
        Guid eventId,
        string tenantId,
        DateTimeOffset occurredAtUtc,
        Guid itemId,
        string sku)
    {
        this.EventId = IntegrationEventContractGuards.RequireId(eventId, nameof(eventId));
        this.TenantId = IntegrationEventContractGuards.NormalizeTenantId(tenantId, nameof(tenantId));
        this.OccurredAtUtc = IntegrationEventContractGuards.RequireOccurredAtUtc(occurredAtUtc, nameof(occurredAtUtc));
        this.ItemId = IntegrationEventContractGuards.RequireId(itemId, nameof(itemId));
        this.Sku = IntegrationEventContractGuards
            .NormalizeRequiredText(sku, CatalogContractLimits.SkuMaxLength, nameof(sku))
            .ToUpperInvariant();
    }

    public Guid EventId { get; }
    public string TenantId { get; }
    public DateTimeOffset OccurredAtUtc { get; }
    public Guid ItemId { get; }
    public string Sku { get; }
    public string EventName => "item-discontinued";
    public int Version => 1;
}
