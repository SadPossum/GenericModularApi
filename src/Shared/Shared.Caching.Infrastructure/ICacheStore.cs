namespace Shared.Caching.Infrastructure;

using Shared.Caching;

internal interface ICacheStore
{
    ValueTask RemoveAsync(CacheKey key, CancellationToken cancellationToken);
    ValueTask RemoveByTagAsync(CacheTag tag, CancellationToken cancellationToken);
}
