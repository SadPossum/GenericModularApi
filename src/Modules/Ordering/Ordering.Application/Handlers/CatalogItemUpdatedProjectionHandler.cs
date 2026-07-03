namespace Ordering.Application.Handlers;

using Catalog.Contracts;
using Ordering.Application.Ports;
using Shared.Application.Messaging;

internal sealed class CatalogItemUpdatedProjectionHandler(ICatalogItemProjectionRepository repository)
    : IIntegrationEventHandler<CatalogItemUpdatedIntegrationEvent>
{
    public async Task HandleAsync(CatalogItemUpdatedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        await repository.UpsertAsync(
            new CatalogItemProjectionWriteModel(
                integrationEvent.TenantId,
                integrationEvent.ItemId,
                integrationEvent.Sku,
                integrationEvent.Name,
                integrationEvent.Price,
                integrationEvent.Currency,
                integrationEvent.Status),
            cancellationToken).ConfigureAwait(false);
    }
}
