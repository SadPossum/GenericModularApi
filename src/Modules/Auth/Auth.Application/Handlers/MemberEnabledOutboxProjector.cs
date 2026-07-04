namespace Auth.Application.Handlers;

using Auth.Contracts;
using Auth.Domain.Events;
using Shared.Application.Events;
using Shared.Messaging;

internal sealed class MemberEnabledOutboxProjector(IOutboxWriterRegistry outboxWriters)
    : IDomainEventHandler<MemberEnabledDomainEvent>
{
    public Task HandleAsync(MemberEnabledDomainEvent domainEvent, CancellationToken cancellationToken) =>
        outboxWriters.GetRequired(AuthModuleMetadata.Name).EnqueueAsync(
            new MemberEnabledIntegrationEvent(
                domainEvent.EventId,
                domainEvent.TenantId,
                domainEvent.OccurredAtUtc,
                domainEvent.MemberId.Value),
            cancellationToken);
}
