namespace Auth.Contracts;

using Shared.Messaging;
using Shared.Tenancy;

[IntegrationEventName(MemberSessionsRevokedIntegrationEvent.EventType)]
[IntegrationEventVersion(MemberSessionsRevokedIntegrationEvent.EventVersion)]
[TenantScoped]
public sealed record MemberSessionsRevokedIntegrationEvent : IntegrationEvent
{
    public const string EventType = "member-sessions-revoked";
    public const int EventVersion = 1;

    public MemberSessionsRevokedIntegrationEvent(
        Guid eventId,
        string tenantId,
        DateTimeOffset occurredAtUtc,
        Guid memberId,
        int revokedSessionCount)
        : base(eventId, tenantId, occurredAtUtc, EventType, EventVersion)
    {
        this.MemberId = IntegrationEventContractGuards.RequireId(memberId, nameof(memberId));
        this.RevokedSessionCount = IntegrationEventContractGuards.RequireNonNegative(
            revokedSessionCount,
            nameof(revokedSessionCount));
    }

    public Guid MemberId { get; }
    public int RevokedSessionCount { get; }
}
