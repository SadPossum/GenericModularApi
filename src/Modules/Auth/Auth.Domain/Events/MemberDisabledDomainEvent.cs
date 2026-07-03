namespace Auth.Domain.Events;

using Auth.Domain.Aggregates;
using Auth.Domain.ValueObjects;
using Shared.Domain;

public sealed record MemberDisabledDomainEvent : TenantDomainEvent
{
    public MemberDisabledDomainEvent(
        Guid eventId,
        DateTimeOffset occurredAtUtc,
        MemberId memberId,
        string tenantId,
        string reason)
        : base(eventId, occurredAtUtc, tenantId)
    {
        _ = DomainEventGuards.RequireId(memberId.Value, nameof(memberId));
        this.MemberId = memberId;
        this.Reason = DomainEventGuards.NormalizeRequiredText(reason, Member.DisabledReasonMaxLength, nameof(reason));
    }

    public MemberId MemberId { get; }
    public string Reason { get; }
}
