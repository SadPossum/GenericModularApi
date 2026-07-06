namespace Ordering.Contracts;

using Catalog.Contracts;
using Notifications.Contracts;
using Shared.Messaging;
using Shared.ModuleComposition;
using Shared.Modules;
using Shared.Tasks;

public static class OrderingModuleMetadata
{
    public const string Name = "ordering";
    public const string Schema = "ordering";
    public const string CatalogItemProjectionName = "catalog-item-projections";
    public const int CatalogItemProjectionVersion = 1;
    public const string ProjectionWorkerGroup = "projection-workers";
    public const string CatalogItemCreatedProjectionHandlerName = "catalog-item-created-projection";
    public const string CatalogItemUpdatedProjectionHandlerName = "catalog-item-updated-projection";
    public const string CatalogItemDiscontinuedProjectionHandlerName = "catalog-item-discontinued-projection";

    public static ModuleDescriptor Descriptor { get; } = ModuleDescriptor
        .Create(Name)
        .WithSchema(Schema)
        .WithSubscription<CatalogItemCreatedIntegrationEvent>(CatalogModuleMetadata.Name, CatalogItemCreatedProjectionHandlerName)
        .WithSubscription<CatalogItemUpdatedIntegrationEvent>(CatalogModuleMetadata.Name, CatalogItemUpdatedProjectionHandlerName)
        .WithSubscription<CatalogItemDiscontinuedIntegrationEvent>(CatalogModuleMetadata.Name, CatalogItemDiscontinuedProjectionHandlerName)
        .WithPublishedEvent<UserNotificationRequestedIntegrationEvent>()
        .WithTask<RebuildCatalogItemProjectionPayload>()
        .WithProfile(OrderingProfiles.Default)
        .Build();
}
