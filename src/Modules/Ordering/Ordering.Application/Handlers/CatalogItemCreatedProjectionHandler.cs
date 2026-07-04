namespace Ordering.Application.Handlers;

using Catalog.Contracts;
using Ordering.Application.Ports;
using Shared.Messaging;

internal sealed class CatalogItemCreatedProjectionHandler(ICatalogItemProjectionRepository repository)
    : IIntegrationEventHandler<CatalogItemCreatedIntegrationEvent>
{
    public async Task HandleAsync(CatalogItemCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        await repository.UpsertAsync(
            new CatalogItemProjectionWriteModel(
                integrationEvent.TenantId,
                integrationEvent.ItemId,
                integrationEvent.Sku,
                integrationEvent.Name,
                integrationEvent.Price,
                integrationEvent.Currency,
                CatalogItemStatus.Active),
            cancellationToken).ConfigureAwait(false);
    }
}
