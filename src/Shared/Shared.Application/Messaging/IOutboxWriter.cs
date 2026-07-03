namespace Shared.Application.Messaging;

public interface IOutboxWriter
{
    string ModuleName { get; }

    Task EnqueueAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken)
        where TEvent : IIntegrationEvent;
}
