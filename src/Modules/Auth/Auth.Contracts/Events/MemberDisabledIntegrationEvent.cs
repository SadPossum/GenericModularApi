namespace Auth.Contracts;

using Shared.Application.Messaging;

public sealed record MemberDisabledIntegrationEvent : IntegrationEvent
{
    public MemberDisabledIntegrationEvent(
        Guid eventId,
        string tenantId,
        DateTimeOffset occurredAtUtc,
        Guid memberId,
        string reason)
        : base(eventId, tenantId, occurredAtUtc, "member-disabled", version: 1)
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
