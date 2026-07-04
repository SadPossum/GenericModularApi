# Caching

Caching is an optional cache-aside optimization. It is disabled by default and never owns authoritative state.

## Boundaries

Modules may depend on the contracts in `Shared.Caching`:

- `IApplicationCache`
- `ICacheInvalidationQueue`
- `CacheKey`
- `CacheTag`
- `CacheEntryPolicy`

Modules must not reference HybridCache, StackExchange.Redis, or `Shared.Caching.Redis`. The host selects the implementation.

Do not cache authorization decisions, refresh tokens, tenant resolution, or other security-sensitive state. Prefer immutable read models and data that can safely be reconstructed from its source.

## Explicit Cache-Aside

Query handlers opt in at the read site:

```csharp
CacheKey key = CacheKey.Tenant("catalog", "product", query.ProductId.ToString());

return await cache.GetOrCreateAsync(
    key,
    token => repository.GetReadModelAsync(query.ProductId, token),
    tags: [CacheTag.Tenant("catalog", "products")],
    cancellationToken: cancellationToken);
```

There is no automatic query caching behavior. The handler remains responsible for deciding whether a result is safe and useful to cache.

## Keys And Tenant Isolation

Modules create logical keys. Infrastructure generates physical keys centrally:

```text
gma:{environment}:{module}:{scope}:{tenant-or-global}:{entry}:{encoded-segments}
```

Use `CacheKey.Tenant` and `CacheTag.Tenant` for tenant-owned data. A tenant-scoped key requires an active tenant when tenancy is enabled, and the active tenant id is normalized through the shared `TenantIds` rules before infrastructure formats the physical key or tag. Use global keys only for data that is identical for every tenant.

Storage prefix and host environment names are normalized to lowercase and validated before physical keys are generated. `Caching:KeyPrefix` must be 1-32 ASCII letters, digits, `-`, or `_`; the host environment segment uses the same character set with a 64-character limit. A key or tag can include up to 16 nonblank, case-preserving segments, with each segment capped at 256 characters before encoding. Segments cannot contain whitespace or control characters. Segments are URI-encoded by infrastructure, and the default physical key limit is 1024 characters. Keys and tenant IDs may appear in structured logs, but never in metric tags.

## Policies

Default policy:

- distributed expiration: 5 minutes;
- local expiration: 30 seconds;
- maximum serialized payload: 1 MB;
- maximum physical key length: 1024 characters.

`CacheEntryPolicy` can override expiration per entry and disable L1 storage. Disable local caching, or use a short local TTL, when cross-node coherence matters more than local latency.

## Invalidation

Command and domain-event handlers enqueue invalidations through `ICacheInvalidationQueue`:

```csharp
invalidationQueue.Remove(CacheKey.Tenant("catalog", "product", productId.ToString()));
invalidationQueue.RemoveByTag(CacheTag.Tenant("catalog", "products"));
```

The cache invalidation command behavior wraps the unit-of-work behavior. It flushes only after all unit-of-work commits succeed. Failed commands and failed commits leave the cache untouched.

Tag invalidation is logical. In Redis mode it is shared through L2, but existing nodes can retain L1 data briefly. TTL remains the final stale-data bound.

## Failure Behavior

Runtime cache failures fail open:

- reads call the source factory;
- backend write failures return the source value;
- invalidation failures do not fail an already committed command;
- factory exceptions and caller cancellation still propagate.

The deferred invalidation queue forgets an entry only after the cache store accepts that invalidation call. If caller cancellation interrupts a flush, unflushed entries remain in the scoped queue and can be retried by the caller.

Malformed cache identities are not fail-open runtime failures. Null keys, null tags, invalid segments, unsupported scopes, and missing tenant context are programming or composition errors and are rejected even when caching is disabled.

Failures are logged and recorded under the `gma.caching` meter. Metric tags are limited to `module`, `operation`, `provider`, and `result`.

Configuration errors fail startup. Redis mode requires the Redis adapter and `ConnectionStrings:redis`.

## Providers

### Disabled

`Caching:Enabled=false` always invokes the source factory.

### Memory

Memory mode uses HybridCache L1 and its in-process stampede protection. It is suitable for a single process or data where per-node cache state is acceptable.

### Redis

Redis mode adds Redis as HybridCache L2 through the separate `Shared.Caching.Redis` adapter. Compose Redis before caching infrastructure:

```csharp
builder.AddRedisCaching();
builder.AddCachingInfrastructure();
```

The adapter is a no-op unless caching is enabled and the provider is `Redis`.
When Redis mode is enabled, `Caching:Redis:ConnectionName`, optional `Caching:Redis:InstanceName`, and the matching `ConnectionStrings:<name>` value are validated at startup. `InstanceName` adds a Redis-provider prefix before the central physical key, so leave it empty unless the Redis database itself needs an extra partition.
