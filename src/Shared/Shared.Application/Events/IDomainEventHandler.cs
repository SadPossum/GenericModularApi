namespace Shared.Application.Events;

using Shared.Domain;

#pragma warning disable CA1711
public interface IDomainEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken);
}
#pragma warning restore CA1711
