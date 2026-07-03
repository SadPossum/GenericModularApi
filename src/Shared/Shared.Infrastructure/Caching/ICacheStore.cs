namespace Shared.Infrastructure.Caching;

using Shared.Application.Caching;

internal interface ICacheStore
{
    ValueTask RemoveAsync(CacheKey key, CancellationToken cancellationToken);
    ValueTask RemoveByTagAsync(CacheTag tag, CancellationToken cancellationToken);
}
