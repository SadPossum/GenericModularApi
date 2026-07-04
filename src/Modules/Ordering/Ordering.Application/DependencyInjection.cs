namespace Ordering.Application;

using Catalog.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Application.Handlers;
using Ordering.Application.Tasks;
using Ordering.Contracts;
using Shared.Application.Composition;
using Shared.Messaging;
using Shared.ProjectionRebuild.Tasks;
using Shared.Tasks;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderingApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddApplicationServicesFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddProjectionRebuildTasks();
        services.AddIntegrationEventHandler<CatalogItemCreatedIntegrationEvent, CatalogItemCreatedProjectionHandler>(
            OrderingModuleMetadata.Name,
            CatalogModuleMetadata.Name);
        services.AddIntegrationEventHandler<CatalogItemUpdatedIntegrationEvent, CatalogItemUpdatedProjectionHandler>(
            OrderingModuleMetadata.Name,
            CatalogModuleMetadata.Name);
        services.AddIntegrationEventHandler<CatalogItemDiscontinuedIntegrationEvent, CatalogItemDiscontinuedProjectionHandler>(
            OrderingModuleMetadata.Name,
            CatalogModuleMetadata.Name);
        services.AddTaskHandler<RebuildCatalogItemProjectionPayload, RebuildCatalogItemProjectionTaskHandler>(OrderingModuleMetadata.Name);

        return services;
    }
}
