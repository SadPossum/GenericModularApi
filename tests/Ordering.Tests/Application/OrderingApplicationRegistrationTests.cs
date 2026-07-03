namespace Ordering.Tests;

using Catalog.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Application;
using Ordering.Application.Commands;
using Ordering.Application.Validation;
using Ordering.Contracts;
using Shared.Application.Cqrs;
using Shared.Application.Messaging;
using Xunit;

[Trait("Category", "Unit")]
public sealed class OrderingApplicationRegistrationTests
{
    [Fact]
    public void Ordering_application_registration_is_idempotent()
    {
        ServiceCollection services = new();

        services.AddOrderingApplication();
        services.AddOrderingApplication();

        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(ICommandHandler<PlaceOrderCommand, OrderDto>));
        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(ICommandValidator<PlaceOrderCommand>));
        Assert.Single(services, descriptor => IsProjectionHandler(descriptor, "CatalogItemCreatedProjectionHandler"));
        Assert.Single(services, descriptor => IsProjectionHandler(descriptor, "CatalogItemUpdatedProjectionHandler"));
        Assert.Single(services, descriptor => IsProjectionHandler(descriptor, "CatalogItemDiscontinuedProjectionHandler"));

        using ServiceProvider provider = services.BuildServiceProvider();
        IIntegrationEventSubscriptionRegistry registry =
            provider.GetRequiredService<IIntegrationEventSubscriptionRegistry>();

        Assert.Contains(registry.Subscriptions, subscription =>
            subscription.EventType == typeof(CatalogItemCreatedIntegrationEvent) &&
            subscription.Subject == CatalogIntegrationSubjects.ItemCreated &&
            subscription.HandlerName == OrderingModuleMetadata.CatalogItemCreatedProjectionHandlerName);
        Assert.Contains(registry.Subscriptions, subscription =>
            subscription.EventType == typeof(CatalogItemUpdatedIntegrationEvent) &&
            subscription.Subject == CatalogIntegrationSubjects.ItemUpdated &&
            subscription.HandlerName == OrderingModuleMetadata.CatalogItemUpdatedProjectionHandlerName);
        Assert.Contains(registry.Subscriptions, subscription =>
            subscription.EventType == typeof(CatalogItemDiscontinuedIntegrationEvent) &&
            subscription.Subject == CatalogIntegrationSubjects.ItemDiscontinued &&
            subscription.HandlerName == OrderingModuleMetadata.CatalogItemDiscontinuedProjectionHandlerName);
    }

    [Fact]
    public void Ordering_application_registration_rejects_null_arguments()
    {
        Assert.Throws<ArgumentNullException>(() => Ordering.Application.DependencyInjection.AddOrderingApplication(null!));
    }

    private static bool IsProjectionHandler(ServiceDescriptor descriptor, string handlerTypeName) =>
        string.Equals(descriptor.ServiceType.Name, handlerTypeName, StringComparison.Ordinal) &&
        string.Equals(descriptor.ServiceType.Namespace, "Ordering.Application.Handlers", StringComparison.Ordinal);
}
