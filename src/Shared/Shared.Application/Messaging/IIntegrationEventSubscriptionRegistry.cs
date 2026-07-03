namespace Shared.Application.Messaging;

public interface IIntegrationEventSubscriptionRegistry
{
    IReadOnlyCollection<IntegrationEventSubscription> Subscriptions { get; }
}
