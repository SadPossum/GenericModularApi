namespace Auth.Domain.Events;

using Auth.Domain.ValueObjects;
using Shared.Domain;

public sealed record MemberSessionsRevokedDomainEvent : TenantDomainEvent
{
    public MemberSessionsRevokedDomainEvent(
        Guid eventId,
        DateTimeOffset occurredAtUtc,
        MemberId memberId,
        string tenantId,
        int revokedSessionCount)
        : base(eventId, occurredAtUtc, tenantId)
    {
        _ = DomainEventGuards.RequireId(memberId.Value, nameof(memberId));
        this.MemberId = memberId;
        this.RevokedSessionCount = DomainEventGuards.RequirePositive(revokedSessionCount, nameof(revokedSessionCount));
    }

    public MemberId MemberId { get; }
    public int RevokedSessionCount { get; }
}
