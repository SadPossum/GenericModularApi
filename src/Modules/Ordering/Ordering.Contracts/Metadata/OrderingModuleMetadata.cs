namespace Ordering.Contracts;

using Catalog.Contracts;
using Shared.Messaging;
using Shared.Modules;
using Shared.Tasks;

public static class OrderingModuleMetadata
{
    public const string Name = "ordering";
    public const string Schema = "ordering";
    public const string CatalogItemProjectionName = "catalog-item-projections";
    public const int CatalogItemProjectionVersion = 1;
    public const string RebuildCatalogItemProjectionsTaskName = "rebuild-catalog-item-projections";
    public const int RebuildCatalogItemProjectionsPayloadVersion = 1;
    public const string ProjectionWorkerGroup = "projection-workers";
    public const string CatalogItemCreatedProjectionHandlerName = "catalog-item-created-projection";
    public const string CatalogItemUpdatedProjectionHandlerName = "catalog-item-updated-projection";
    public const string CatalogItemDiscontinuedProjectionHandlerName = "catalog-item-discontinued-projection";

    public static ModuleDescriptor Descriptor { get; } = ModuleDescriptor
        .Create(Name)
        .WithSchema(Schema)
        .WithSubscriptions([
            new ModuleSubscriptionDescriptor(
                CatalogModuleMetadata.Name,
                "item-created",
                CatalogIntegrationSubjects.ItemCreated,
                CatalogItemCreatedProjectionHandlerName,
                tenantScoped: true),
            new ModuleSubscriptionDescriptor(
                CatalogModuleMetadata.Name,
                "item-updated",
                CatalogIntegrationSubjects.ItemUpdated,
                CatalogItemUpdatedProjectionHandlerName,
                tenantScoped: true),
            new ModuleSubscriptionDescriptor(
                CatalogModuleMetadata.Name,
                "item-discontinued",
                CatalogIntegrationSubjects.ItemDiscontinued,
                CatalogItemDiscontinuedProjectionHandlerName,
                tenantScoped: true),
        ])
        .WithTask(
            new ModuleTaskDescriptor(
                RebuildCatalogItemProjectionsTaskName,
                "Rebuild Ordering's local catalog item projection from Catalog exports.",
                ModuleTaskKind.OneShot,
                tenantScoped: true,
                supportsControlMessages: true,
                ProjectionWorkerGroup,
                RebuildCatalogItemProjectionsPayloadVersion))
        .Build();
}
