namespace Integration.Tests;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Application;
using Shared.Application.Caching;
using Shared.Application.Cqrs;
using Shared.Caching.Redis;
using Shared.ErrorHandling;
using Shared.Infrastructure;
using Xunit;

[Trait("Category", "Integration")]
public sealed class RedisCachingIntegrationTests
{
    [DockerFact]
    [Trait("Category", "Docker")]
    public async Task Redis_provides_cross_instance_reads_expiration_and_invalidation()
    {
        await using IContainer redis = new ContainerBuilder("redis:7.4-alpine")
            .WithPortBinding(6379, assignRandomHostPort: true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(6379))
            .Build();
        await redis.StartAsync();
        string connectionString = $"localhost:{redis.GetMappedPublicPort(6379)}";
        await using ServiceProvider firstProvider = BuildProvider(connectionString);
        await using ServiceProvider secondProvider = BuildProvider(connectionString);
        CacheEntryPolicy distributedOnly = new(
            TimeSpan.FromMinutes(1),
            TimeSpan.FromSeconds(1),
            localCacheEnabled: false);

        using IServiceScope firstScope = firstProvider.CreateScope();
        using IServiceScope secondScope = secondProvider.CreateScope();
        IApplicationCache first = firstScope.ServiceProvider.GetRequiredService<IApplicationCache>();
        IApplicationCache second = secondScope.ServiceProvider.GetRequiredService<IApplicationCache>();
        IDistributedCache configuredDistributedCache = firstScope.ServiceProvider.GetRequiredService<IDistributedCache>();
        Assert.Contains("Redis", configuredDistributedCache.GetType().Name, StringComparison.Ordinal);

        CacheKey sharedKey = CacheKey.Global("catalog", "cross-instance");
        int sharedCalls = 0;
        int created = await first.GetOrCreateAsync(
            sharedKey,
            _ => ValueTask.FromResult(++sharedCalls),
            distributedOnly);
        await WaitForDistributedValueAsync(
            configuredDistributedCache,
            "gma:redistests:catalog:global:global:cross-instance");
        int distributedHit = await second.GetOrCreateAsync(
            sharedKey,
            _ => ValueTask.FromResult(++sharedCalls),
            distributedOnly);
        Assert.Equal(created, distributedHit);
        Assert.Equal(1, sharedCalls);

        CacheKey expiringKey = CacheKey.Global("catalog", "expiring");
        CacheEntryPolicy expiring = new(
            TimeSpan.FromSeconds(1),
            TimeSpan.FromMilliseconds(100),
            localCacheEnabled: false);
        int expirationCalls = 0;
        await first.GetOrCreateAsync(expiringKey, _ => ValueTask.FromResult(++expirationCalls), expiring);
        await WaitForDistributedValueAsync(
            configuredDistributedCache,
            "gma:redistests:catalog:global:global:expiring");
        await Task.Delay(1200);
        await second.GetOrCreateAsync(expiringKey, _ => ValueTask.FromResult(++expirationCalls), expiring);
        Assert.Equal(2, expirationCalls);

        CacheKey invalidatedKey = CacheKey.Global("catalog", "invalidate-key");
        int keyCalls = 0;
        await first.GetOrCreateAsync(invalidatedKey, _ => ValueTask.FromResult(++keyCalls), distributedOnly);
        await WaitForDistributedValueAsync(
            configuredDistributedCache,
            "gma:redistests:catalog:global:global:invalidate-key");
        await second.GetOrCreateAsync(invalidatedKey, _ => ValueTask.FromResult(++keyCalls), distributedOnly);
        ICacheInvalidationQueue firstQueue = firstScope.ServiceProvider.GetRequiredService<ICacheInvalidationQueue>();
        firstQueue.Remove(invalidatedKey);
        await SendSuccessfulCommandAsync(firstScope.ServiceProvider);
        await second.GetOrCreateAsync(invalidatedKey, _ => ValueTask.FromResult(++keyCalls), distributedOnly);
        Assert.Equal(2, keyCalls);

        CacheTag products = CacheTag.Global("catalog", "products");
        CacheKey taggedKey = CacheKey.Global("catalog", "tagged", "42");
        int tagCalls = 0;
        await first.GetOrCreateAsync(taggedKey, _ => ValueTask.FromResult(++tagCalls), distributedOnly, [products]);
        await WaitForDistributedValueAsync(
            configuredDistributedCache,
            "gma:redistests:catalog:global:global:tagged:42");
        await second.GetOrCreateAsync(taggedKey, _ => ValueTask.FromResult(++tagCalls), distributedOnly, [products]);
        const string tagMarkerKey = "__MSFT_HCT__gma:redistests:catalog:global:global:products";
        byte[]? tagMarkerBefore = await configuredDistributedCache.GetAsync(tagMarkerKey);
        await Task.Delay(100);
        firstQueue.RemoveByTag(products);
        await SendSuccessfulCommandAsync(firstScope.ServiceProvider);
        await WaitForDistributedChangeAsync(
            configuredDistributedCache,
            tagMarkerKey,
            tagMarkerBefore);
        await first.GetOrCreateAsync(taggedKey, _ => ValueTask.FromResult(++tagCalls), distributedOnly, [products]);
        Assert.Equal(2, tagCalls);
    }

    private static ServiceProvider BuildProvider(string connectionString)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.Environment.EnvironmentName = "RedisTests";
        builder.Configuration["Caching:Enabled"] = "true";
        builder.Configuration["Caching:Provider"] = "Redis";
        builder.Configuration["Caching:DefaultDistributedExpiration"] = "00:05:00";
        builder.Configuration["Caching:DefaultLocalExpiration"] = "00:00:30";
        builder.Configuration["ConnectionStrings:redis"] = connectionString;
        builder.Configuration["Tenancy:Enabled"] = "false";
        builder.AddRedisCaching();
        builder.AddSharedInfrastructure();
        builder.Services.AddScoped<ICommandHandler<FlushInvalidationsCommand, Unit>, FlushInvalidationsHandler>();
        return builder.Services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }

    private static async Task SendSuccessfulCommandAsync(IServiceProvider provider)
    {
        IRequestDispatcher dispatcher = provider.GetRequiredService<IRequestDispatcher>();
        await dispatcher.SendAsync(new FlushInvalidationsCommand());
    }

    private static async Task WaitForDistributedValueAsync(IDistributedCache cache, string key)
    {
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(5));

        while (await cache.GetAsync(key, timeout.Token) is null)
        {
            await Task.Delay(25, timeout.Token);
        }
    }

    private static async Task WaitForDistributedChangeAsync(
        IDistributedCache cache,
        string key,
        byte[]? previousValue)
    {
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(5));

        while (true)
        {
            byte[]? currentValue = await cache.GetAsync(key, timeout.Token);

            if (currentValue is not null &&
                (previousValue is null || !currentValue.AsSpan().SequenceEqual(previousValue)))
            {
                return;
            }

            await Task.Delay(25, timeout.Token);
        }
    }

    private sealed record FlushInvalidationsCommand : ICommand<Unit>;

    private sealed class FlushInvalidationsHandler : ICommandHandler<FlushInvalidationsCommand, Unit>
    {
        public Task<Result<Unit>> HandleAsync(
            FlushInvalidationsCommand command,
            CancellationToken cancellationToken) =>
            Task.FromResult(Result.Success(Unit.Value));
    }
}
