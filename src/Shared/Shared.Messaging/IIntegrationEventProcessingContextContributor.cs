namespace Shared.Messaging;

public interface IIntegrationEventProcessingContextContributor
{
    void Prepare(IntegrationEventSubscription subscription, IIntegrationEvent integrationEvent);
}
