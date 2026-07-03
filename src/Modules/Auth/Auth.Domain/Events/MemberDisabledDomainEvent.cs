namespace Auth.Domain.Events;

using Auth.Domain.Aggregates;
using Auth.Domain.ValueObjects;
using Shared.Domain;

public sealed record MemberDisabledDomainEvent : IDomainEvent
{
    public MemberDisabledDomainEvent(
        Guid eventId,
        DateTimeOffset occurredAtUtc,
        MemberId memberId,
        string tenantId,
        string reason)
    {
        this.EventId = DomainEventGuards.RequireId(eventId, nameof(eventId));
        this.OccurredAtUtc = DomainEventGuards.RequireOccurredAtUtc(occurredAtUtc, nameof(occurredAtUtc));
        _ = DomainEventGuards.RequireId(memberId.Value, nameof(memberId));
        this.MemberId = memberId;
        this.TenantId = DomainEventGuards.NormalizeTenantId(tenantId, nameof(tenantId));
        this.Reason = DomainEventGuards.NormalizeRequiredText(reason, Member.DisabledReasonMaxLength, nameof(reason));
    }

    public Guid EventId { get; }
    public DateTimeOffset OccurredAtUtc { get; }
    public MemberId MemberId { get; }
    public string TenantId { get; }
    public string Reason { get; }
}
