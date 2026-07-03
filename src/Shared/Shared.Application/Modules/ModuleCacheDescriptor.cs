namespace Shared.Application.Modules;

using Shared.Application.Caching;

public sealed record ModuleCacheDescriptor
{
    public ModuleCacheDescriptor(string name, string scope, bool tenantScoped, IReadOnlyList<string> tags)
    {
        this.Name = CacheIdentity.ValidateName(name, nameof(name));
        this.Scope = ModuleMetadataNaming.NormalizeCacheScope(scope, nameof(scope));
        this.TenantScoped = tenantScoped;
        this.Tags = Array.AsReadOnly((tags ?? throw new ArgumentNullException(nameof(tags)))
            .Select(tag => CacheIdentity.ValidateName(tag, nameof(tags)))
            .ToArray());

        if (this.Tags.Count == 0)
        {
            throw new ArgumentException("Cache metadata must declare at least one tag.", nameof(tags));
        }

        bool tenantScopedByScope = this.Scope == "tenant";
        if (tenantScopedByScope != this.TenantScoped)
        {
            throw new ArgumentException("Cache metadata scope and tenant flag must agree.", nameof(tenantScoped));
        }

        ModuleMetadataNaming.EnsureUnique(this.Tags, tag => tag, "cache tag");
    }

    public string Name { get; }
    public string Scope { get; }
    public bool TenantScoped { get; }
    public IReadOnlyList<string> Tags { get; }
}
