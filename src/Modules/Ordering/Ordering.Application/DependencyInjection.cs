namespace Ordering.Application;

using Catalog.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Application.Handlers;
using Ordering.Application.Tasks;
using Ordering.Contracts;
using Shared.Application.Composition;
using Shared.Messaging;
using Shared.ProjectionRebuild;
using Shared.Tasks;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderingApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddApplicationServicesFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddProjectionRebuild();
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
        services.AddTaskHandler<RebuildCatalogItemProjectionPayload, RebuildCatalogItemProjectionTaskHandler>(
            OrderingModuleMetadata.Name,
            OrderingModuleMetadata.RebuildCatalogItemProjectionsTaskName,
            OrderingModuleMetadata.ProjectionWorkerGroup,
            tenantScoped: true,
            payloadVersion: OrderingModuleMetadata.RebuildCatalogItemProjectionsPayloadVersion,
            kind: ModuleTaskKind.OneShot,
            supportsControlMessages: true);

        return services;
    }
}
