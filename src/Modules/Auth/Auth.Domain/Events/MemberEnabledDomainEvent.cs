namespace Auth.Domain.Events;

using Auth.Domain.ValueObjects;
using Shared.Domain;

public sealed record MemberEnabledDomainEvent : TenantDomainEvent
{
    public MemberEnabledDomainEvent(
        Guid eventId,
        DateTimeOffset occurredAtUtc,
        MemberId memberId,
        string tenantId)
        : base(eventId, occurredAtUtc, tenantId)
        => this.MemberId = new MemberId(DomainEventGuards.RequireId(memberId.Value, nameof(memberId)));

    public MemberId MemberId { get; }
}
