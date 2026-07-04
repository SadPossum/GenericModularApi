namespace Shared.Messaging;

public interface IIntegrationEventSubscriptionRegistry
{
    IReadOnlyCollection<IntegrationEventSubscription> Subscriptions { get; }
}
