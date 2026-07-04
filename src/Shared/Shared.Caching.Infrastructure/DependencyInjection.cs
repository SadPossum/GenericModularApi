namespace Shared.Caching.Infrastructure;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Caching;
using Shared.Runtime.Infrastructure;
using Shared.Tenancy.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddCachingInfrastructure(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        CachingOptions cachingOptions = builder.Configuration
            .GetSection(CachingOptions.SectionName)
            .Get<CachingOptions>() ?? new CachingOptions();
        ValidateOptionsResult validation = new CachingOptionsValidator().Validate(name: null, cachingOptions);
        if (validation.Failed)
        {
            throw new OptionsValidationException(
                CachingOptions.SectionName,
                typeof(CachingOptions),
                validation.Failures);
        }

        builder.AddRuntimeInfrastructure();
        builder.AddTenancyInfrastructure();

        if (builder.Services.Any(descriptor => descriptor.ServiceType == typeof(CachingInfrastructureRegistrationMarker)))
        {
            return builder;
        }

        builder.Services.AddSingleton<CachingInfrastructureRegistrationMarker>();
        builder.Services
            .AddOptions<CachingOptions>()
            .Bind(builder.Configuration.GetSection(CachingOptions.SectionName))
            .ValidateOnStart();
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<CachingOptions>, CachingOptionsValidator>());
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<CachingOptions>, CachingCompositionOptionsValidator>());
        builder.Services.AddHybridCache(options =>
        {
            options.MaximumPayloadBytes = cachingOptions.MaximumPayloadBytes;
            options.MaximumKeyLength = cachingOptions.MaximumKeyLength;
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = cachingOptions.DefaultDistributedExpiration,
                LocalCacheExpiration = cachingOptions.DefaultLocalExpiration
            };
        });
        builder.Services.TryAddSingleton<CacheMetrics>();
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, HybridCacheMetricsLoggerProvider>());
        builder.Services.TryAddScoped<CacheKeyFormatter>();
        builder.Services.TryAddScoped<HybridApplicationCache>();
        builder.Services.TryAddScoped<IApplicationCache>(provider => provider.GetRequiredService<HybridApplicationCache>());
        builder.Services.TryAddScoped<ICacheStore>(provider => provider.GetRequiredService<HybridApplicationCache>());
        builder.Services.TryAddScoped<CacheInvalidationQueue>();
        builder.Services.TryAddScoped<ICacheInvalidationQueue>(provider => provider.GetRequiredService<CacheInvalidationQueue>());
        builder.Services.TryAddScoped<ICacheInvalidationQueueFlusher>(provider => provider.GetRequiredService<CacheInvalidationQueue>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, CachingStartupValidator>());

        return builder;
    }

    private sealed class CachingInfrastructureRegistrationMarker;
}
