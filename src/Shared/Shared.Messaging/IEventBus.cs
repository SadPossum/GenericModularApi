namespace Shared.Messaging;

public interface IEventBus
{
    Task PublishAsync(OutboxMessageRecord message, CancellationToken cancellationToken);
}
