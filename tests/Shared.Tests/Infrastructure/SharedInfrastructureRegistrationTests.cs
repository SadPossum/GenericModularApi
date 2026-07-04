namespace Shared.Tests;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shared.Application.Events;
using Shared.Application.Events.Infrastructure;
using Shared.Cqrs;
using Shared.Caching;
using Shared.Messaging;
using Shared.Messaging.Nats;
using Shared.Runtime.Identity;
using Shared.Runtime.Time;
using Shared.Tasks;
using Shared.Tasks.Cqrs;
using Shared.Tenancy;
using Shared.Results;
using Shared.Infrastructure;
using Shared.Caching.Infrastructure;
using Shared.Cqrs.Infrastructure;
using Shared.Messaging.Infrastructure;
using Shared.Runtime.Infrastructure;
using Shared.Tasks.Infrastructure;
using Shared.Tenancy.Infrastructure;
using Xunit;
using ApplicationEventsDependencyInjection = Shared.Application.Events.Infrastructure.DependencyInjection;
using CachingDependencyInjection = Shared.Caching.Infrastructure.DependencyInjection;
using CoreDependencyInjection = Shared.Infrastructure.DependencyInjection;
using CqrsDependencyInjection = Shared.Cqrs.Infrastructure.DependencyInjection;
using MessagingDependencyInjection = Shared.Messaging.Infrastructure.DependencyInjection;
using NatsDependencyInjection = Shared.Messaging.Nats.DependencyInjection;
using RuntimeDependencyInjection = Shared.Runtime.Infrastructure.DependencyInjection;
using TaskCqrsDependencyInjection = Shared.Tasks.Cqrs.DependencyInjection;
using TaskDependencyInjection = Shared.Tasks.Infrastructure.TaskWorkerRuntimeDependencyInjection;
using TenancyDependencyInjection = Shared.Tenancy.Infrastructure.DependencyInjection;

[Trait("Category", "Unit")]
public sealed class SharedInfrastructureRegistrationTests
{
    [Fact]
    public void Shared_infrastructure_registration_rejects_null_arguments()
    {
        Assert.Throws<ArgumentNullException>(() => CoreDependencyInjection.AddSharedInfrastructure(null!));
        Assert.Throws<ArgumentNullException>(() => TenancyDependencyInjection.AddTenancyInfrastructure(null!));
        Assert.Throws<ArgumentNullException>(() => RuntimeDependencyInjection.AddRuntimeInfrastructure(null!));
        Assert.Throws<ArgumentNullException>(() => ApplicationEventsDependencyInjection.AddApplicationEventsInfrastructure(null!));
        Assert.Throws<ArgumentNullException>(() => CqrsDependencyInjection.AddCqrsInfrastructure(null!));
        Assert.Throws<ArgumentNullException>(() => CachingDependencyInjection.AddCachingInfrastructure(null!));
        Assert.Throws<ArgumentNullException>(() => MessagingDependencyInjection.AddMessagingInfrastructure(null!));
        Assert.Throws<ArgumentNullException>(() => MessagingDependencyInjection.AddOutboxPublishing(null!));
        Assert.Throws<ArgumentNullException>(() => NatsDependencyInjection.AddNatsJetStreamMessaging(null!));
        Assert.Throws<ArgumentNullException>(() => NatsDependencyInjection.AddNatsJetStreamConsumers(null!));
        Assert.Throws<ArgumentNullException>(() => TaskCqrsDependencyInjection.AddTaskCqrs(null!));
        Assert.Throws<ArgumentNullException>(() => TaskDependencyInjection.AddTaskInfrastructure(null!));
    }

    [Fact]
    public void Shared_infrastructure_registration_is_idempotent()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        builder.AddRuntimeInfrastructure();
        builder.AddRuntimeInfrastructure();
        builder.AddApplicationEventsInfrastructure();
        builder.AddApplicationEventsInfrastructure();
        builder.AddCqrsInfrastructure();
        builder.AddCqrsInfrastructure();
        builder.AddSharedInfrastructure();
        builder.AddSharedInfrastructure();
        builder.AddTenancyInfrastructure();

        Assert.Single(builder.Services, HasService<IValidateOptions<TenantOptions>, TenantOptionsValidator>());
        Assert.Single(builder.Services, descriptor => descriptor.ServiceType.Name == "TenancyInfrastructureRegistrationMarker");
        Assert.Single(builder.Services, descriptor => descriptor.ServiceType.Name == "RuntimeInfrastructureRegistrationMarker");
        Assert.Single(builder.Services, descriptor => descriptor.ServiceType.Name == "ApplicationEventsInfrastructureRegistrationMarker");
        Assert.Single(builder.Services, descriptor => descriptor.ServiceType.Name == "CqrsInfrastructureRegistrationMarker");
        Assert.Single(builder.Services, descriptor => descriptor.ServiceType.Name == "SharedInfrastructureRegistrationMarker");
        Assert.Single(builder.Services, descriptor => descriptor.ServiceType == typeof(IIdGenerator));
        Assert.Single(builder.Services, descriptor => descriptor.ServiceType == typeof(ISystemClock));
        Assert.Single(builder.Services, descriptor => descriptor.ServiceType == typeof(IDomainEventDispatcher));
        Assert.Single(builder.Services, descriptor => descriptor.ServiceType == typeof(IRequestDispatcher));
        Assert.Single(builder.Services, HasOpenGenericService(typeof(ICommandPipelineBehavior<,>), typeof(ValidationCommandBehavior<,>)));
        Assert.Single(builder.Services, HasOpenGenericService(typeof(ICommandPipelineBehavior<,>), typeof(CommandUnitOfWorkBehavior<,>)));
        Assert.Single(builder.Services, HasOpenGenericService(typeof(IQueryPipelineBehavior<,>), typeof(ValidationQueryBehavior<,>)));
        Assert.DoesNotContain(builder.Services, HasService<IValidateOptions<CachingOptions>, CachingOptionsValidator>());
        Assert.DoesNotContain(builder.Services, HasService<IValidateOptions<OutboxOptions>, OutboxOptionsValidator>());
        Assert.DoesNotContain(builder.Services, HasService<ITaskCommandDispatcher, TaskCommandDispatcher>());
        Assert.DoesNotContain(builder.Services, HasService<IHostedService, CachingStartupValidator>());
    }

    [Theory]
    [InlineData("Caching:DefaultDistributedExpiration", "00:00:00", "DefaultDistributedExpiration")]
    [InlineData("Caching:DefaultLocalExpiration", "00:10:00", "DefaultLocalExpiration")]
    [InlineData("Caching:MaximumPayloadBytes", "0", "MaximumPayloadBytes")]
    [InlineData("Caching:MaximumKeyLength", "0", "MaximumKeyLength")]
    [InlineData("Caching:KeyPrefix", "gma.value", "KeyPrefix")]
    public void Caching_infrastructure_rejects_invalid_caching_options_before_hybrid_cache_registration(
        string setting,
        string value,
        string expectedFailure)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.Configuration[setting] = value;

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
            builder.AddCachingInfrastructure());

        Assert.Contains(exception.Failures, failure => failure.Contains(expectedFailure, StringComparison.Ordinal));
        Assert.DoesNotContain(builder.Services, descriptor => descriptor.ServiceType.FullName?.Contains("HybridCache", StringComparison.Ordinal) == true);
        Assert.DoesNotContain(builder.Services, descriptor => descriptor.ServiceType.Name == "CachingInfrastructureRegistrationMarker");
    }

    [Theory]
    [InlineData("Tenancy:HeaderName", "X Tenant Id", "HeaderName")]
    public void Shared_infrastructure_rejects_invalid_core_options_before_service_mutation(
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

    [Theory]
    [InlineData("Outbox:BatchSize", "0", "BatchSize")]
    public void Messaging_infrastructure_rejects_invalid_runtime_options_before_service_mutation(
        string setting,
        string value,
        string expectedFailure)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.Configuration[setting] = value;

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
            builder.AddMessagingInfrastructure());

        Assert.Contains(exception.Failures, failure => failure.Contains(expectedFailure, StringComparison.Ordinal));
        Assert.DoesNotContain(builder.Services, descriptor => descriptor.ServiceType.Name == "MessagingInfrastructureRegistrationMarker");
        Assert.DoesNotContain(builder.Services, descriptor => descriptor.ServiceType == typeof(IRequestDispatcher));
    }

    [Theory]
    [InlineData("NatsJetStream:StreamName", "GMA.EVENTS", "StreamName")]
    [InlineData("NatsConsumers:DurablePrefix", "gma.prod", "DurablePrefix")]
    public void Nats_adapter_rejects_invalid_runtime_options_before_service_mutation(
        string setting,
        string value,
        string expectedFailure)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.Configuration[setting] = value;

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
            builder.AddNatsJetStreamMessaging());

        Assert.Contains(exception.Failures, failure => failure.Contains(expectedFailure, StringComparison.Ordinal));
        Assert.DoesNotContain(builder.Services, descriptor => descriptor.ServiceType.Name == "MessagingInfrastructureRegistrationMarker");
        Assert.DoesNotContain(builder.Services, descriptor => descriptor.ServiceType == typeof(IRequestDispatcher));
        Assert.DoesNotContain(builder.Services, HasService<IEventBus, NatsJetStreamEventBus>());
    }

    [Fact]
    public void Messaging_host_service_registration_is_idempotent()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

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

        builder.AddNatsJetStreamMessaging();
        builder.AddNatsJetStreamMessaging();

        Assert.Single(builder.Services, HasService<IEventBus, NatsJetStreamEventBus>());
        Assert.Single(builder.Services, HasService<IHostedService, OutboxPublisherService>());
    }

    [Theory]
    [InlineData("outbox")]
    [InlineData("nats-publisher")]
    [InlineData("nats-consumer")]
    public void Messaging_runtime_registration_composes_only_runtime_infrastructure_dependencies(
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

        Assert.Single(builder.Services, descriptor => descriptor.ServiceType.Name == "RuntimeInfrastructureRegistrationMarker");
        Assert.Single(builder.Services, descriptor => descriptor.ServiceType == typeof(IIdGenerator));
        Assert.Single(builder.Services, descriptor => descriptor.ServiceType == typeof(ISystemClock));
        Assert.DoesNotContain(builder.Services, descriptor => descriptor.ServiceType.Name == "SharedInfrastructureRegistrationMarker");
        Assert.DoesNotContain(builder.Services, descriptor => descriptor.ServiceType == typeof(IRequestDispatcher));
    }

    [Fact]
    public async Task Task_command_dispatcher_delegates_to_shared_request_dispatcher()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.AddTaskCqrs();
        builder.AddTaskCqrs();
        builder.Services.AddScoped<ICommandHandler<TestTaskCommand, Unit>, TestTaskCommandHandler>();

        using IHost host = builder.Build();

        ITaskCommandDispatcher dispatcher = host.Services.GetRequiredService<ITaskCommandDispatcher>();
        TaskExecutionContext context = new(
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            "catalog",
            "rebuild-search",
            TaskWorkerGroups.Default,
            "worker-01",
            "node-01",
            attempt: 1);

        Result<Unit> result = await dispatcher.DispatchAsync<TestTaskCommand, Unit>(
            context,
            new TestTaskCommand(),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Task_runtime_infrastructure_does_not_compose_cqrs_dispatch()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        builder.AddTaskInfrastructure();
        builder.AddTaskInfrastructure();

        Assert.Single(builder.Services, descriptor => descriptor.ServiceType.Name == "TaskInfrastructureRegistrationMarker");
        Assert.Single(builder.Services, descriptor => descriptor.ServiceType.Name == "RuntimeInfrastructureRegistrationMarker");
        Assert.DoesNotContain(builder.Services, descriptor => descriptor.ServiceType.Name == "CqrsInfrastructureRegistrationMarker");
        Assert.DoesNotContain(builder.Services, descriptor => descriptor.ServiceType == typeof(IRequestDispatcher));
        Assert.DoesNotContain(builder.Services, descriptor => descriptor.ServiceType == typeof(ITaskCommandDispatcher));
    }

    private static Predicate<ServiceDescriptor> HasService<TService, TImplementation>() =>
        descriptor =>
            descriptor.ServiceType == typeof(TService) &&
            descriptor.ImplementationType == typeof(TImplementation);

    private static Predicate<ServiceDescriptor> HasOpenGenericService(Type serviceType, Type implementationType) =>
        descriptor =>
            descriptor.ServiceType == serviceType &&
            descriptor.ImplementationType == implementationType;

    private sealed record TestTaskCommand : ICommand<Unit>;

    private sealed class TestTaskCommandHandler : ICommandHandler<TestTaskCommand, Unit>
    {
        public Task<Result<Unit>> HandleAsync(TestTaskCommand command, CancellationToken cancellationToken) =>
            Task.FromResult(Result.Success(Unit.Value));
    }
}
