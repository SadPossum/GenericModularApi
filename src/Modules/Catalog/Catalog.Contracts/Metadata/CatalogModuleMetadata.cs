namespace Catalog.Contracts;

using Shared.Application.Modules;

public static class CatalogModuleMetadata
{
    public const string Name = "catalog";
    public const string Schema = "catalog";
    public const string ItemsCacheName = "catalog-items";
    public const string ItemsCacheTag = "catalog.items";
    public const string ItemsCacheEntry = "items";
    public const string ItemCacheEntry = "item";

    public static ModuleDescriptor Descriptor { get; } = new(
        Name,
        Schema,
        [
            new ModulePermissionDescriptor(CatalogAdminPermissionCodes.ItemsRead, "Read catalog items.", tenantScoped: true),
            new ModulePermissionDescriptor(CatalogAdminPermissionCodes.ItemsCreate, "Create catalog items.", tenantScoped: true),
            new ModulePermissionDescriptor(CatalogAdminPermissionCodes.ItemsUpdate, "Update catalog items.", tenantScoped: true),
            new ModulePermissionDescriptor(CatalogAdminPermissionCodes.ItemsDiscontinue, "Discontinue catalog items.", tenantScoped: true),
        ],
        [
            new ModuleIntegrationEventDescriptor("item-created", CatalogIntegrationSubjects.ItemCreated, 1, tenantScoped: true),
            new ModuleIntegrationEventDescriptor("item-updated", CatalogIntegrationSubjects.ItemUpdated, 1, tenantScoped: true),
            new ModuleIntegrationEventDescriptor("item-discontinued", CatalogIntegrationSubjects.ItemDiscontinued, 1, tenantScoped: true),
        ],
        [],
        [
            new ModuleCacheDescriptor(ItemsCacheName, "tenant", tenantScoped: true, [ItemsCacheTag]),
        ]);
}

