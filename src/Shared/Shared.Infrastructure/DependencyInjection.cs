namespace Shared.Infrastructure;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shared.Application.Caching;
using Shared.Application.Cqrs;
using Shared.Application.Events;
using Shared.Application.Identity;
using Shared.Application.Messaging;
using Shared.Application.Tenancy;
using Shared.Infrastructure.Caching;
using Shared.Infrastructure.Cqrs;
using Shared.Infrastructure.Events;
using Shared.Infrastructure.Identity;
using Shared.Infrastructure.Messaging;
using Shared.Infrastructure.Observability;
using Shared.Infrastructure.Tenancy;
using Shared.Infrastructure.Time;
using Shared.Application.Time;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddSharedInfrastructure(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.Services.Any(descriptor => descriptor.ServiceType == typeof(SharedInfrastructureRegistrationMarker)))
        {
            return builder;
        }

        CachingOptions cachingOptions = ValidateOptions(
            builder.Configuration,
            CachingOptions.SectionName,
            new CachingOptions(),
            new CachingOptionsValidator());
        ValidateOptions(
            builder.Configuration,
            TenantOptions.SectionName,
            new TenantOptions(),
            new TenantOptionsValidator());
        ValidateOptions(
            builder.Configuration,
            OutboxOptions.SectionName,
            new OutboxOptions(),
            new OutboxOptionsValidator());
        ValidateOptions(
            builder.Configuration,
            NatsJetStreamOptions.SectionName,
            new NatsJetStreamOptions(),
            new NatsJetStreamOptionsValidator());
        ValidateOptions(
            builder.Configuration,
            NatsConsumerOptions.SectionName,
            new NatsConsumerOptions(),
            new NatsConsumerOptionsValidator());

        builder.Services.AddSingleton<SharedInfrastructureRegistrationMarker>();
        builder.Services.AddMetrics();
        builder.Services
            .AddOptions<CachingOptions>()
            .Bind(builder.Configuration.GetSection(CachingOptions.SectionName))
            .ValidateOnStart();
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<Microsoft.Extensions.Options.IValidateOptions<CachingOptions>, CachingOptionsValidator>());
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<Microsoft.Extensions.Options.IValidateOptions<CachingOptions>, CachingCompositionOptionsValidator>());
        builder.Services.AddHybridCache(options =>
        {
            options.MaximumPayloadBytes = cachingOptions.MaximumPayloadBytes;
            options.MaximumKeyLength = cachingOptions.MaximumKeyLength;
            options.DefaultEntryOptions = new Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions
            {
                Expiration = cachingOptions.DefaultDistributedExpiration,
                LocalCacheExpiration = cachingOptions.DefaultLocalExpiration
            };
        });
        builder.Services
            .AddOptions<TenantOptions>()
            .Bind(builder.Configuration.GetSection(TenantOptions.SectionName))
            .ValidateOnStart();
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<TenantOptions>, TenantOptionsValidator>());
        builder.Services
            .AddOptions<OutboxOptions>()
            .Bind(builder.Configuration.GetSection(OutboxOptions.SectionName))
            .ValidateOnStart();
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<OutboxOptions>, OutboxOptionsValidator>());
        builder.Services
            .AddOptions<NatsJetStreamOptions>()
            .Bind(builder.Configuration.GetSection(NatsJetStreamOptions.SectionName))
            .ValidateOnStart();
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<NatsJetStreamOptions>, NatsJetStreamOptionsValidator>());
        builder.Services
            .AddOptions<NatsConsumerOptions>()
            .Bind(builder.Configuration.GetSection(NatsConsumerOptions.SectionName))
            .ValidateOnStart();
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<NatsConsumerOptions>, NatsConsumerOptionsValidator>());
        builder.Services.TryAddScoped<NullTenantContext>();
        builder.Services.TryAddScoped<ITenantContext>(provider => provider.GetRequiredService<NullTenantContext>());
        builder.Services.TryAddScoped<ITenantContextAccessor>(provider => provider.GetRequiredService<NullTenantContext>());
        builder.Services.TryAddSingleton<IIdGenerator, GuidIdGenerator>();
        builder.Services.TryAddSingleton<ISystemClock, SystemClock>();
        builder.Services.TryAddScoped<IRequestDispatcher, RequestDispatcher>();
        builder.Services.TryAddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        builder.Services.TryAddSingleton<CommandMetrics>();
        builder.Services.TryAddSingleton<QueryMetrics>();
        builder.Services.TryAddSingleton<OutboxMetrics>();
        builder.Services.TryAddSingleton<InboxMetrics>();
        builder.Services.TryAddSingleton<CacheMetrics>();
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<Microsoft.Extensions.Logging.ILoggerProvider, HybridCacheMetricsLoggerProvider>());
        builder.Services.TryAddScoped<CacheKeyFormatter>();
        builder.Services.TryAddScoped<HybridApplicationCache>();
        builder.Services.TryAddScoped<IApplicationCache>(provider => provider.GetRequiredService<HybridApplicationCache>());
        builder.Services.TryAddScoped<ICacheStore>(provider => provider.GetRequiredService<HybridApplicationCache>());
        builder.Services.TryAddScoped<CacheInvalidationQueue>();
        builder.Services.TryAddScoped<ICacheInvalidationQueue>(provider => provider.GetRequiredService<CacheInvalidationQueue>());
        builder.Services.TryAddScoped<ICacheInvalidationQueueFlusher>(provider => provider.GetRequiredService<CacheInvalidationQueue>());
        builder.Services.AddHostedService<CachingStartupValidator>();
        builder.Services.TryAddEnumerable(ServiceDescriptor.Scoped(typeof(ICommandPipelineBehavior<,>), typeof(ValidationCommandBehavior<,>)));
        builder.Services.TryAddEnumerable(ServiceDescriptor.Scoped(typeof(ICommandPipelineBehavior<,>), typeof(LoggingCommandBehavior<,>)));
        builder.Services.TryAddEnumerable(ServiceDescriptor.Scoped(typeof(ICommandPipelineBehavior<,>), typeof(CacheInvalidationCommandBehavior<,>)));
        builder.Services.TryAddEnumerable(ServiceDescriptor.Scoped(typeof(ICommandPipelineBehavior<,>), typeof(CommandUnitOfWorkBehavior<,>)));
        builder.Services.TryAddEnumerable(ServiceDescriptor.Scoped(typeof(IQueryPipelineBehavior<,>), typeof(ValidationQueryBehavior<,>)));
        builder.Services.TryAddEnumerable(ServiceDescriptor.Scoped(typeof(IQueryPipelineBehavior<,>), typeof(LoggingQueryBehavior<,>)));
        builder.Services.TryAddScoped<IOutboxWriterRegistry, OutboxWriterRegistry>();
        builder.Services.TryAddSingleton<IEventBus, NullEventBus>();

        return builder;
    }

    private static TOptions ValidateOptions<TOptions>(
        IConfiguration configuration,
        string sectionName,
        TOptions fallbackOptions,
        IValidateOptions<TOptions> validator)
        where TOptions : class
    {
        TOptions options = configuration
            .GetSection(sectionName)
            .Get<TOptions>() ?? fallbackOptions;
        ValidateOptionsResult result = validator.Validate(name: null, options);

        if (result.Failed)
        {
            throw new OptionsValidationException(sectionName, typeof(TOptions), result.Failures);
        }

        return options;
    }

    public static IHostApplicationBuilder AddNatsJetStreamMessaging(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddSharedInfrastructure();
        builder.Services.Replace(ServiceDescriptor.Singleton<IEventBus, NatsJetStreamEventBus>());
        builder.AddOutboxPublishing();
        return builder;
    }

    public static IHostApplicationBuilder AddOutboxPublishing(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddSharedInfrastructure();
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, OutboxPublisherService>());
        return builder;
    }

    public static IHostApplicationBuilder AddNatsJetStreamConsumers(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddSharedInfrastructure();
        builder.Services.TryAddSingleton<IIntegrationEventSubscriptionRegistry, IntegrationEventSubscriptionRegistry>();
        builder.Services.AddHostedService<NatsJetStreamConsumerService>();
        return builder;
    }

    private sealed class SharedInfrastructureRegistrationMarker;
}
