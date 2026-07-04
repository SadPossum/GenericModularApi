namespace Ordering.Application.Handlers;

using Catalog.Contracts;
using Ordering.Application.Ports;
using Ordering.Contracts;
using Shared.Messaging;

[IntegrationEventHandler(OrderingModuleMetadata.CatalogItemUpdatedProjectionHandlerName)]
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
