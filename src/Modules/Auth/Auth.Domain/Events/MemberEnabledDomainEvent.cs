namespace Auth.Domain.Events;

using Auth.Domain.ValueObjects;
using Shared.Domain;

public sealed record MemberEnabledDomainEvent : IDomainEvent
{
    public MemberEnabledDomainEvent(
        Guid eventId,
        DateTimeOffset occurredAtUtc,
        MemberId memberId,
        string tenantId)
    {
        this.EventId = DomainEventGuards.RequireId(eventId, nameof(eventId));
        this.OccurredAtUtc = DomainEventGuards.RequireOccurredAtUtc(occurredAtUtc, nameof(occurredAtUtc));
        _ = DomainEventGuards.RequireId(memberId.Value, nameof(memberId));
        this.MemberId = memberId;
        this.TenantId = DomainEventGuards.NormalizeTenantId(tenantId, nameof(tenantId));
    }

    public Guid EventId { get; }
    public DateTimeOffset OccurredAtUtc { get; }
    public MemberId MemberId { get; }
    public string TenantId { get; }
}
