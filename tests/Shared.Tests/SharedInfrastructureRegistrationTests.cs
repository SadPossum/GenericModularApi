namespace Shared.Tests;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shared.Application.Caching;
using Shared.Application.Cqrs;
using Shared.Application.Messaging;
using Shared.Application.Tenancy;
using Shared.Infrastructure;
using Shared.Infrastructure.Caching;
using Shared.Infrastructure.Cqrs;
using Shared.Infrastructure.Messaging;
using Shared.Infrastructure.Tenancy;
using Xunit;

[Trait("Category", "Unit")]
public sealed class SharedInfrastructureRegistrationTests
{
    [Fact]
    public void Shared_infrastructure_registration_rejects_null_arguments()
    {
        Assert.Throws<ArgumentNullException>(() => DependencyInjection.AddSharedInfrastructure(null!));
        Assert.Throws<ArgumentNullException>(() => DependencyInjection.AddOutboxPublishing(null!));
        Assert.Throws<ArgumentNullException>(() => DependencyInjection.AddNatsJetStreamMessaging(null!));
        Assert.Throws<ArgumentNullException>(() => DependencyInjection.AddNatsJetStreamConsumers(null!));
    }

    [Fact]
    public void Shared_infrastructure_registration_is_idempotent()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        builder.AddSharedInfrastructure();
        builder.AddSharedInfrastructure();

        Assert.Single(builder.Services, HasService<IValidateOptions<CachingOptions>, CachingOptionsValidator>());
        Assert.Single(builder.Services, HasService<IValidateOptions<CachingOptions>, CachingCompositionOptionsValidator>());
        Assert.Single(builder.Services, HasService<IValidateOptions<TenantOptions>, TenantOptionsValidator>());
        Assert.Single(builder.Services, HasService<IValidateOptions<OutboxOptions>, OutboxOptionsValidator>());
        Assert.Single(builder.Services, HasService<IValidateOptions<NatsJetStreamOptions>, NatsJetStreamOptionsValidator>());
        Assert.Single(builder.Services, HasService<IValidateOptions<NatsConsumerOptions>, NatsConsumerOptionsValidator>());
        Assert.Single(builder.Services, HasService<IHostedService, CachingStartupValidator>());
        Assert.Single(builder.Services, HasOpenGenericService(typeof(ICommandPipelineBehavior<,>), typeof(ValidationCommandBehavior<,>)));
        Assert.Single(builder.Services, HasOpenGenericService(typeof(ICommandPipelineBehavior<,>), typeof(CommandUnitOfWorkBehavior<,>)));
        Assert.Single(builder.Services, HasOpenGenericService(typeof(IQueryPipelineBehavior<,>), typeof(ValidationQueryBehavior<,>)));
    }

    [Theory]
    [InlineData("Caching:DefaultDistributedExpiration", "00:00:00", "DefaultDistributedExpiration")]
    [InlineData("Caching:DefaultLocalExpiration", "00:10:00", "DefaultLocalExpiration")]
    [InlineData("Caching:MaximumPayloadBytes", "0", "MaximumPayloadBytes")]
    [InlineData("Caching:MaximumKeyLength", "0", "MaximumKeyLength")]
    [InlineData("Caching:KeyPrefix", "gma.value", "KeyPrefix")]
    public void Shared_infrastructure_rejects_invalid_caching_options_before_hybrid_cache_registration(
        string setting,
        string value,
        string expectedFailure)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.Configuration[setting] = value;

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
            builder.AddSharedInfrastructure());

        Assert.Contains(exception.Failures, failure => failure.Contains(expectedFailure, StringComparison.Ordinal));
        Assert.DoesNotContain(builder.Services, descriptor => descriptor.ServiceType.FullName?.Contains("HybridCache", StringComparison.Ordinal) == true);
        Assert.DoesNotContain(builder.Services, descriptor => descriptor.ServiceType.Name == "SharedInfrastructureRegistrationMarker");
    }

    [Theory]
    [InlineData("Tenancy:HeaderName", "X Tenant Id", "HeaderName")]
    [InlineData("Outbox:BatchSize", "0", "BatchSize")]
    [InlineData("NatsJetStream:StreamName", "GMA.EVENTS", "StreamName")]
    [InlineData("NatsConsumers:DurablePrefix", "gma.prod", "DurablePrefix")]
    public void Shared_infrastructure_rejects_invalid_runtime_options_before_service_mutation(
        string setting,
        string value,
        string expectedFailure)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.Configuration[setting] = value;

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
            builder.AddSharedInfrastructure());

        Assert.Contains(exception.Failures, failure => failure.Contains(expectedFailure, StringComparison.Ordinal));
        Assert.DoesNotContain(builder.Services, descriptor => descriptor.ServiceType.Name == "SharedInfrastructureRegistrationMarker");
        Assert.DoesNotContain(builder.Services, descriptor => descriptor.ServiceType == typeof(IRequestDispatcher));
    }

    [Fact]
    public void Messaging_host_service_registration_is_idempotent()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        builder.AddSharedInfrastructure();
        builder.AddOutboxPublishing();
        builder.AddOutboxPublishing();
        builder.AddNatsJetStreamConsumers();
        builder.AddNatsJetStreamConsumers();

        Assert.Single(builder.Services, HasService<IHostedService, OutboxPublisherService>());
        Assert.Single(builder.Services, HasService<IHostedService, NatsJetStreamConsumerService>());
        Assert.Single(builder.Services, descriptor => descriptor.ServiceType == typeof(IIntegrationEventSubscriptionRegistry));
    }

    [Fact]
    public void Nats_messaging_registration_is_idempotent()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        builder.AddSharedInfrastructure();
        builder.AddNatsJetStreamMessaging();
        builder.AddNatsJetStreamMessaging();

        Assert.Single(builder.Services, HasService<IEventBus, NatsJetStreamEventBus>());
        Assert.Single(builder.Services, HasService<IHostedService, OutboxPublisherService>());
    }

    [Theory]
    [InlineData("outbox")]
    [InlineData("nats-publisher")]
    [InlineData("nats-consumer")]
    public void Messaging_runtime_registration_composes_shared_infrastructure_dependencies(
        string registration)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        switch (registration)
        {
            case "outbox":
                builder.AddOutboxPublishing();
                break;
            case "nats-publisher":
                builder.AddNatsJetStreamMessaging();
                break;
            case "nats-consumer":
                builder.AddNatsJetStreamConsumers();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(registration), registration, "Unknown registration.");
        }

        Assert.Single(builder.Services, descriptor => descriptor.ServiceType.Name == "SharedInfrastructureRegistrationMarker");
        Assert.Single(builder.Services, descriptor => descriptor.ServiceType == typeof(IRequestDispatcher));
    }

    private static Predicate<ServiceDescriptor> HasService<TService, TImplementation>() =>
        descriptor =>
            descriptor.ServiceType == typeof(TService) &&
            descriptor.ImplementationType == typeof(TImplementation);

    private static Predicate<ServiceDescriptor> HasOpenGenericService(Type serviceType, Type implementationType) =>
        descriptor =>
            descriptor.ServiceType == serviceType &&
            descriptor.ImplementationType == implementationType;
}
