namespace Auth.Application.Handlers;

using Auth.Contracts;
using Auth.Domain.Events;
using Shared.Application.Events;
using Shared.Application.Messaging;

internal sealed class MemberRegisteredOutboxProjector(IOutboxWriterRegistry outboxWriters)
    : IDomainEventHandler<MemberRegisteredDomainEvent>
{
    public Task HandleAsync(MemberRegisteredDomainEvent domainEvent, CancellationToken cancellationToken) =>
        outboxWriters.GetRequired(AuthModuleMetadata.Name).EnqueueAsync(
            new MemberRegisteredIntegrationEvent(
                domainEvent.EventId,
                domainEvent.TenantId,
                domainEvent.OccurredAtUtc,
                domainEvent.MemberId.Value,
                domainEvent.Username),
            cancellationToken);
}
