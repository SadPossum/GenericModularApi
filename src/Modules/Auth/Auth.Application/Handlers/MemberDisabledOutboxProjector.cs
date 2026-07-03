namespace Auth.Application.Handlers;

using Auth.Contracts;
using Auth.Domain.Events;
using Shared.Application.Events;
using Shared.Application.Messaging;

internal sealed class MemberDisabledOutboxProjector(IOutboxWriterRegistry outboxWriters)
    : IDomainEventHandler<MemberDisabledDomainEvent>
{
    public Task HandleAsync(MemberDisabledDomainEvent domainEvent, CancellationToken cancellationToken) =>
        outboxWriters.GetRequired(AuthModuleMetadata.Name).EnqueueAsync(
            new MemberDisabledIntegrationEvent(
                domainEvent.EventId,
                domainEvent.TenantId,
                domainEvent.OccurredAtUtc,
                domainEvent.MemberId.Value,
                domainEvent.Reason),
            cancellationToken);
}
