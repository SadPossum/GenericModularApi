namespace Shared.Application.Modules;

public sealed record ModuleDescriptor
{
    public ModuleDescriptor(
        string name,
        string? schema,
        IReadOnlyList<ModulePermissionDescriptor> permissions,
        IReadOnlyList<ModuleIntegrationEventDescriptor> publishedEvents,
        IReadOnlyList<ModuleSubscriptionDescriptor> subscriptions,
        IReadOnlyList<ModuleCacheDescriptor> cacheEntries,
        string? adminSurfaceName = null)
    {
        this.Name = ModuleMetadataNaming.NormalizeModuleName(name, nameof(name));
        this.Schema = string.IsNullOrWhiteSpace(schema)
            ? null
            : ModuleMetadataNaming.NormalizeModuleName(schema, nameof(schema));
        this.AdminSurfaceName = string.IsNullOrWhiteSpace(adminSurfaceName)
            ? null
            : ModuleMetadataNaming.NormalizeModuleName(adminSurfaceName, nameof(adminSurfaceName));
        this.Permissions = ModuleMetadataNaming.CopyRequiredList(permissions, nameof(permissions));
        this.PublishedEvents = ModuleMetadataNaming.CopyRequiredList(publishedEvents, nameof(publishedEvents));
        this.Subscriptions = ModuleMetadataNaming.CopyRequiredList(subscriptions, nameof(subscriptions));
        this.CacheEntries = ModuleMetadataNaming.CopyRequiredList(cacheEntries, nameof(cacheEntries));

        foreach (ModuleIntegrationEventDescriptor publishedEvent in this.PublishedEvents)
        {
            string expectedSubject = Shared.Application.Messaging.IntegrationEventNaming.CreateSubject(
                "gma",
                this.Name,
                publishedEvent.EventType,
                publishedEvent.Version);

            if (!string.Equals(publishedEvent.Subject, expectedSubject, StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    $"Published event '{publishedEvent.EventType}' subject must match the module name and event version.",
                    nameof(publishedEvents));
            }
        }

        ModuleMetadataNaming.EnsureUnique(this.Permissions, permission => permission.Code, "permission");
        ModuleMetadataNaming.EnsureUnique(this.PublishedEvents, publishedEvent => publishedEvent.Subject, "published event subject");
        ModuleMetadataNaming.EnsureUnique(this.Subscriptions, subscription => $"{subscription.ProducerModule}.{subscription.HandlerName}", "subscription handler");
        ModuleMetadataNaming.EnsureUnique(this.CacheEntries, cacheEntry => cacheEntry.Name, "cache entry");
    }

    public string Name { get; }
    public string? Schema { get; }
    public IReadOnlyList<ModulePermissionDescriptor> Permissions { get; }
    public IReadOnlyList<ModuleIntegrationEventDescriptor> PublishedEvents { get; }
    public IReadOnlyList<ModuleSubscriptionDescriptor> Subscriptions { get; }
    public IReadOnlyList<ModuleCacheDescriptor> CacheEntries { get; }
    public string? AdminSurfaceName { get; }

    public static ModuleDescriptor Empty(string name, string? schema = null, string? adminSurfaceName = null) =>
        new(name, schema, [], [], [], [], adminSurfaceName);
}
