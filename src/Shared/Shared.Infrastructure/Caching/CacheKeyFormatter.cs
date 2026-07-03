namespace Shared.Infrastructure.Caching;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shared.Application.Caching;
using Shared.Application.Tenancy;
using Shared.Domain;

internal sealed class CacheKeyFormatter(
    ITenantContext tenantContext,
    IHostEnvironment environment,
    IOptions<CachingOptions> options)
{
    private readonly CachingOptions cachingOptions = options.Value;

    public string Format(CacheKey key) => this.Format(key.Module, key.Entry, key.Scope, key.Segments);

    public string Format(CacheTag tag) => this.Format(tag.Module, tag.Entry, tag.Scope, tag.Segments);

    private string Format(string module, string entry, CacheScope scope, IReadOnlyList<string> segments)
    {
        string tenant = scope switch
        {
            CacheScope.Global => "global",
            CacheScope.Tenant => this.ResolveTenant(),
            _ => throw new InvalidOperationException($"Unsupported cache scope '{scope}'.")
        };
        string scopeName = scope.ToString().ToLowerInvariant();
        IEnumerable<string> parts = new[]
        {
            CacheStorageIdentifiers.NormalizeKeyPrefix(this.cachingOptions.KeyPrefix),
            CacheStorageIdentifiers.NormalizeEnvironmentName(environment.EnvironmentName),
            module,
            scopeName,
            tenant,
            entry
        }.Concat(segments).Select(Uri.EscapeDataString);

        string physicalKey = string.Join(':', parts);

        if (physicalKey.Length > this.cachingOptions.MaximumKeyLength)
        {
            throw new InvalidOperationException(
                $"The generated cache key exceeds the configured {this.cachingOptions.MaximumKeyLength}-character limit.");
        }

        return physicalKey;
    }

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
