namespace Auth.Contracts;

using Shared.Messaging;

public sealed record MemberEnabledIntegrationEvent : IntegrationEvent
{
    public MemberEnabledIntegrationEvent(
        Guid eventId,
        string tenantId,
        DateTimeOffset occurredAtUtc,
        Guid memberId)
        : base(eventId, tenantId, occurredAtUtc, "member-enabled", version: 1)
        => this.MemberId = IntegrationEventContractGuards.RequireId(memberId, nameof(memberId));

    public Guid MemberId { get; }
}
