namespace Auth.Contracts;

using Shared.Application.Messaging;

public sealed record MemberDisabledIntegrationEvent : IIntegrationEvent
{
    public MemberDisabledIntegrationEvent(
        Guid eventId,
        string tenantId,
        DateTimeOffset occurredAtUtc,
        Guid memberId,
        string reason)
    {
        this.EventId = IntegrationEventContractGuards.RequireId(eventId, nameof(eventId));
        this.TenantId = IntegrationEventContractGuards.NormalizeTenantId(tenantId, nameof(tenantId));
        this.OccurredAtUtc = IntegrationEventContractGuards.RequireOccurredAtUtc(occurredAtUtc, nameof(occurredAtUtc));
        this.MemberId = IntegrationEventContractGuards.RequireId(memberId, nameof(memberId));
        this.Reason = IntegrationEventContractGuards.NormalizeRequiredText(
            reason,
            AuthContractLimits.DisableReasonMaxLength,
            nameof(reason));
    }

    public Guid EventId { get; }
    public string TenantId { get; }
    public DateTimeOffset OccurredAtUtc { get; }
    public Guid MemberId { get; }
    public string Reason { get; }
    public string EventName => "member-disabled";
    public int Version => 1;
}
