namespace Auth.Contracts;

using Shared.Messaging;

public sealed record MemberSessionsRevokedIntegrationEvent : IntegrationEvent
{
    public MemberSessionsRevokedIntegrationEvent(
        Guid eventId,
        string tenantId,
        DateTimeOffset occurredAtUtc,
        Guid memberId,
        int revokedSessionCount)
        : base(eventId, tenantId, occurredAtUtc, "member-sessions-revoked", version: 1)
    {
        this.MemberId = IntegrationEventContractGuards.RequireId(memberId, nameof(memberId));
        this.RevokedSessionCount = IntegrationEventContractGuards.RequireNonNegative(
            revokedSessionCount,
            nameof(revokedSessionCount));
    }

    public Guid MemberId { get; }
    public int RevokedSessionCount { get; }
}
