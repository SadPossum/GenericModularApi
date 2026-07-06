namespace Shared.Tenancy.Caching;

using Shared.Caching;
using Shared.Caching.Infrastructure;
using Shared.Naming;
using Shared.Tenancy;

internal sealed class TenantCacheScopeValueResolver(ITenantContext tenantContext) : ICacheScopeValueResolver
{
    public string Resolve(CacheScope scope) =>
        scope switch
        {
            CacheScope.Global => "global",
            CacheScope.Tenant => this.ResolveTenant(),
            _ => throw new InvalidOperationException($"Unsupported cache scope '{scope}'.")
        };

    private string ResolveTenant()
    {
        if (!tenantContext.IsEnabled)
        {
            return "default";
        }

        if (string.IsNullOrWhiteSpace(tenantContext.TenantId))
        {
            throw new InvalidOperationException("A tenant-scoped cache key requires an active tenant.");
        }

        return TenantIds.Normalize(tenantContext.TenantId);
    }
}
