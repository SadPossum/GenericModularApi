namespace Auth.Contracts;

using Shared.Messaging;
using Shared.Tenancy;

[IntegrationEventName(MemberDisabledIntegrationEvent.EventType)]
[IntegrationEventVersion(MemberDisabledIntegrationEvent.EventVersion)]
[TenantScoped]
public sealed record MemberDisabledIntegrationEvent : IntegrationEvent
{
    public const string EventType = "member-disabled";
    public const int EventVersion = 1;

    public MemberDisabledIntegrationEvent(
        Guid eventId,
        string tenantId,
        DateTimeOffset occurredAtUtc,
        Guid memberId,
        string reason)
        : base(eventId, tenantId, occurredAtUtc, EventType, EventVersion)
    {
        this.MemberId = IntegrationEventContractGuards.RequireId(memberId, nameof(memberId));
        this.Reason = IntegrationEventContractGuards.NormalizeRequiredText(
            reason,
            AuthContractLimits.DisableReasonMaxLength,
            nameof(reason));
    }

    public Guid MemberId { get; }
    public string Reason { get; }
}
