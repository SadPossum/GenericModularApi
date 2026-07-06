namespace Shared.Caching.Infrastructure;

using Shared.Caching;

public interface ICacheScopeValueResolver
{
    string Resolve(CacheScope scope);
}
