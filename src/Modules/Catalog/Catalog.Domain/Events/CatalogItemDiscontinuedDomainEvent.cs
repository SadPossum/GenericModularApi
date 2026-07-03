namespace Catalog.Domain.Events;

using Catalog.Domain.Aggregates;
using Shared.Domain;

public sealed record CatalogItemDiscontinuedDomainEvent : IDomainEvent
{
    public CatalogItemDiscontinuedDomainEvent(
        Guid eventId,
        DateTimeOffset occurredAtUtc,
        Guid itemId,
        string tenantId,
        string sku)
    {
        this.EventId = DomainEventGuards.RequireId(eventId, nameof(eventId));
        this.OccurredAtUtc = DomainEventGuards.RequireOccurredAtUtc(occurredAtUtc, nameof(occurredAtUtc));
        this.ItemId = DomainEventGuards.RequireId(itemId, nameof(itemId));
        this.TenantId = DomainEventGuards.NormalizeTenantId(tenantId, nameof(tenantId));
        this.Sku = DomainEventGuards
            .NormalizeRequiredText(sku, CatalogItem.SkuMaxLength, nameof(sku))
            .ToUpperInvariant();
    }

    public Guid EventId { get; }
    public DateTimeOffset OccurredAtUtc { get; }
    public Guid ItemId { get; }
    public string TenantId { get; }
    public string Sku { get; }
}
