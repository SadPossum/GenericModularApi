namespace Auth.Domain.Events;

using Auth.Domain.ValueObjects;
using Shared.Domain;

public sealed record MemberSessionsRevokedDomainEvent : IDomainEvent
{
    public MemberSessionsRevokedDomainEvent(
        Guid eventId,
        DateTimeOffset occurredAtUtc,
        MemberId memberId,
        string tenantId,
        int revokedSessionCount)
    {
        this.EventId = DomainEventGuards.RequireId(eventId, nameof(eventId));
        this.OccurredAtUtc = DomainEventGuards.RequireOccurredAtUtc(occurredAtUtc, nameof(occurredAtUtc));
        _ = DomainEventGuards.RequireId(memberId.Value, nameof(memberId));
        this.MemberId = memberId;
        this.TenantId = DomainEventGuards.NormalizeTenantId(tenantId, nameof(tenantId));
        this.RevokedSessionCount = DomainEventGuards.RequirePositive(revokedSessionCount, nameof(revokedSessionCount));
    }

    public Guid EventId { get; }
    public DateTimeOffset OccurredAtUtc { get; }
    public MemberId MemberId { get; }
    public string TenantId { get; }
    public int RevokedSessionCount { get; }
}
