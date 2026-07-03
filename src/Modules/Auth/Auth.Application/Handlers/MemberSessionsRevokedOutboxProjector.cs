namespace Auth.Application.Handlers;

using Auth.Contracts;
using Auth.Domain.Events;
using Shared.Application.Events;
using Shared.Application.Messaging;

internal sealed class MemberSessionsRevokedOutboxProjector(IOutboxWriterRegistry outboxWriters)
    : IDomainEventHandler<MemberSessionsRevokedDomainEvent>
{
    public Task HandleAsync(MemberSessionsRevokedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        outboxWriters.GetRequired(AuthModuleMetadata.Name).EnqueueAsync(
            new MemberSessionsRevokedIntegrationEvent(
                domainEvent.EventId,
                domainEvent.TenantId,
                domainEvent.OccurredAtUtc,
                domainEvent.MemberId.Value,
                domainEvent.RevokedSessionCount),
            cancellationToken);
}
