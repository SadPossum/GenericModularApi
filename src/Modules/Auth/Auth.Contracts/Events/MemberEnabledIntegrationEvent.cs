namespace Auth.Contracts;

using Shared.Messaging;
using Shared.Tenancy;

[IntegrationEventName(MemberEnabledIntegrationEvent.EventType)]
[IntegrationEventVersion(MemberEnabledIntegrationEvent.EventVersion)]
[TenantScoped]
public sealed record MemberEnabledIntegrationEvent : IntegrationEvent
{
    public const string EventType = "member-enabled";
    public const int EventVersion = 1;

    public MemberEnabledIntegrationEvent(
        Guid eventId,
        string tenantId,
        DateTimeOffset occurredAtUtc,
        Guid memberId)
        : base(eventId, tenantId, occurredAtUtc, EventType, EventVersion)
        => this.MemberId = IntegrationEventContractGuards.RequireId(memberId, nameof(memberId));

    public Guid MemberId { get; }
}
