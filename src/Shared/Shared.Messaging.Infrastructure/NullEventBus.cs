namespace Shared.Messaging.Infrastructure;

using Shared.Messaging;

internal sealed class NullEventBus : IEventBus
{
    public Task PublishAsync(OutboxMessageRecord message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        throw new InvalidOperationException(
            "No integration event bus is configured. Register a concrete messaging adapter before starting outbox publishing.");
    }
}
