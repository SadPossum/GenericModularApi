namespace Shared.Tests;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shared.Application.Messaging;
using Shared.Infrastructure.Messaging;
using Shared.Messaging.Nats.Aspire;
using Xunit;

[Trait("Category", "Unit")]
public sealed class NatsAspireAdapterTests
{
    [Fact]
    public void Configured_nats_adapter_rejects_null_builder()
    {
        Assert.Throws<ArgumentNullException>(() => DependencyInjection.AddConfiguredNatsJetStreamMessaging(null!));
    }

    [Fact]
    public void Configured_nats_adapter_is_noop_when_disabled()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.Configuration["NatsJetStream:Enabled"] = "false";

        builder.AddConfiguredNatsJetStreamMessaging();

        Assert.DoesNotContain(builder.Services, descriptor => descriptor.ServiceType == typeof(IEventBus));
    }

    [Fact]
    public void Configured_nats_adapter_rejects_invalid_stream_name_at_composition()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.Configuration["NatsJetStream:Enabled"] = "true";
        builder.Configuration["NatsJetStream:StreamName"] = "GMA.EVENTS";
        builder.Configuration["ConnectionStrings:nats"] = "nats://localhost:4222";

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
            builder.AddConfiguredNatsJetStreamMessaging());

        Assert.Contains(exception.Failures, failure => failure.Contains("StreamName", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Configured_nats_adapter_requires_connection_when_enabled(string? connectionString)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.Configuration["NatsJetStream:Enabled"] = "true";
        builder.Configuration["NatsJetStream:StreamName"] = "GMA_EVENTS";
        builder.Configuration["ConnectionStrings:nats"] = connectionString;

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
            builder.AddConfiguredNatsJetStreamMessaging());

        Assert.Contains(exception.Failures, failure => failure.Contains("ConnectionStrings:nats", StringComparison.Ordinal));
    }

    [Fact]
    public void Configured_nats_adapter_registers_event_bus_and_publisher_when_enabled()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        Shared.Infrastructure.DependencyInjection.AddSharedInfrastructure(builder);
        builder.Configuration["NatsJetStream:Enabled"] = "true";
        builder.Configuration["NatsJetStream:StreamName"] = "GMA_EVENTS";
        builder.Configuration["ConnectionStrings:nats"] = "nats://localhost:4222";

        builder.AddConfiguredNatsJetStreamMessaging();

        Assert.Single(builder.Services, descriptor =>
            descriptor.ServiceType == typeof(IEventBus) &&
            descriptor.ImplementationType == typeof(NatsJetStreamEventBus));
        Assert.Single(builder.Services, descriptor =>
            descriptor.ServiceType == typeof(IHostedService) &&
            descriptor.ImplementationType == typeof(OutboxPublisherService));
    }
}
