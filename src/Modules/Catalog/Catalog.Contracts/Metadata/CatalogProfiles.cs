namespace Catalog.Contracts;

using Shared.Caching;
using Shared.Messaging;
using Shared.ModuleComposition;
using Shared.Tenancy;

public static class CatalogProfiles
{
    public const string DefaultName = "default";

    public static ModuleProfileDescriptor Default { get; } = new(
        CatalogModuleMetadata.Name,
        DefaultName,
        provides:
        [
            CatalogCompositionFeatures.ItemsProvided(Provider(DefaultName))
        ],
        requires:
        [
            new RequiredCompositionFeature(
                TenancyCompositionFeatures.Context,
                Provider(DefaultName),
                reason: "Catalog is tenant-scoped; register TenancyModule or at least Shared.Tenancy.Infrastructure."),
            CachingCompositionFeatures.ApplicationRequired(
                Provider(DefaultName),
                "Catalog read handlers use explicit cache-aside; register Shared.Caching.Infrastructure or Shared.Caching.Cqrs."),
            CachingCompositionFeatures.InvalidationRequired(
                Provider(DefaultName),
                "Catalog commands enqueue post-commit cache invalidations; register Shared.Caching.Infrastructure or Shared.Caching.Cqrs."),
            MessagingCompositionFeatures.OutboxRequired(
                Provider(DefaultName),
                "Catalog publishes integration events through its module outbox; register Shared.Messaging.Infrastructure.")
        ],
        displayName: "Catalog default",
        description: "Tenant-scoped catalog item management with explicit cache-aside and producer-owned outbox events.");

    private static string Provider(string profileName) => $"{CatalogModuleMetadata.Name}/{profileName}";
}
