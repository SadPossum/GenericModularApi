namespace Ordering.Application;

using Catalog.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Application.Handlers;
using Ordering.Contracts;
using Shared.Application.Composition;
using Shared.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderingApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddApplicationServicesFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddIntegrationEventHandler<CatalogItemCreatedIntegrationEvent, CatalogItemCreatedProjectionHandler>(
            OrderingModuleMetadata.Name,
            CatalogIntegrationSubjects.ItemCreated,
            OrderingModuleMetadata.CatalogItemCreatedProjectionHandlerName);
        services.AddIntegrationEventHandler<CatalogItemUpdatedIntegrationEvent, CatalogItemUpdatedProjectionHandler>(
            OrderingModuleMetadata.Name,
            CatalogIntegrationSubjects.ItemUpdated,
            OrderingModuleMetadata.CatalogItemUpdatedProjectionHandlerName);
        services.AddIntegrationEventHandler<CatalogItemDiscontinuedIntegrationEvent, CatalogItemDiscontinuedProjectionHandler>(
            OrderingModuleMetadata.Name,
            CatalogIntegrationSubjects.ItemDiscontinued,
            OrderingModuleMetadata.CatalogItemDiscontinuedProjectionHandlerName);

        return services;
    }
}
