namespace Shared.Infrastructure.Caching;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

internal static class CachingCompositionGuard
{
    private const string MissingRedisAdapterMessage =
        "Redis caching is enabled but no distributed cache adapter is registered. Call AddRedisCaching() before AddSharedInfrastructure().";

    public static CachingOptions EnsureValid(CachingOptions options, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        if (RequiresRedisAdapter(options) && serviceProvider.GetService<IDistributedCache>() is null)
        {
            throw new InvalidOperationException(MissingRedisAdapterMessage);
        }

        return options;
    }

    public static CachingOptions EnsureValid(
        CachingOptions options,
        IEnumerable<IDistributedCache> distributedCaches)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(distributedCaches);

        if (RequiresRedisAdapter(options) && !distributedCaches.Any())
        {
            throw new InvalidOperationException(MissingRedisAdapterMessage);
        }

        return options;
    }

    private static bool RequiresRedisAdapter(CachingOptions options) =>
        options.Enabled && options.Provider == CacheProvider.Redis;
}
