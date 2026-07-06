namespace Catalog.Application.Handlers;

using Catalog.Application.Mapping;
using Catalog.Contracts;
using Catalog.Domain.Events;
using Shared.Application.Events;
using Shared.Messaging;

internal sealed class CatalogItemUpdatedOutboxProjector(IOutboxWriterRegistry outboxWriters)
    : IDomainEventHandler<CatalogItemUpdatedDomainEvent>
{
    public Task HandleAsync(CatalogItemUpdatedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        outboxWriters.GetRequired(CatalogModuleMetadata.Name).EnqueueAsync(
            new CatalogItemUpdatedIntegrationEvent(
                domainEvent.EventId,
                domainEvent.TenantId,
                domainEvent.OccurredAtUtc,
                domainEvent.ItemId,
                domainEvent.Sku,
                domainEvent.Name,
                domainEvent.Price,
                domainEvent.Currency,
                CatalogItemMapper.ToContractStatus(domainEvent.Status),
                domainEvent.AvailableRegions),
            cancellationToken);
}
