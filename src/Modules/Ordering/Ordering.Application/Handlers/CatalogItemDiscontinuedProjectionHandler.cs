namespace Ordering.Application.Handlers;

using Catalog.Contracts;
using Ordering.Application.Ports;
using Shared.Messaging;

internal sealed class CatalogItemDiscontinuedProjectionHandler(ICatalogItemProjectionRepository repository)
    : IIntegrationEventHandler<CatalogItemDiscontinuedIntegrationEvent>
{
    public async Task HandleAsync(CatalogItemDiscontinuedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        await repository.MarkDiscontinuedAsync(integrationEvent.TenantId, integrationEvent.ItemId, cancellationToken)
            .ConfigureAwait(false);
    }
}
