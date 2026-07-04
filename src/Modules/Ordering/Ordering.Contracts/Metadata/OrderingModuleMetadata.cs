namespace Ordering.Contracts;

using Catalog.Contracts;
using Shared.Messaging;
using Shared.Modules;

public static class OrderingModuleMetadata
{
    public const string Name = "ordering";
    public const string Schema = "ordering";
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
        .Build();
}
