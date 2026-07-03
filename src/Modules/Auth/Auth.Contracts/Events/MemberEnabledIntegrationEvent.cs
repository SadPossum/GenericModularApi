namespace Auth.Contracts;

using Shared.Application.Messaging;

public sealed record MemberEnabledIntegrationEvent : IIntegrationEvent
{
    public MemberEnabledIntegrationEvent(
        Guid eventId,
        string tenantId,
        DateTimeOffset occurredAtUtc,
        Guid memberId)
    {
        this.EventId = IntegrationEventContractGuards.RequireId(eventId, nameof(eventId));
        this.TenantId = IntegrationEventContractGuards.NormalizeTenantId(tenantId, nameof(tenantId));
        this.OccurredAtUtc = IntegrationEventContractGuards.RequireOccurredAtUtc(occurredAtUtc, nameof(occurredAtUtc));
        this.MemberId = IntegrationEventContractGuards.RequireId(memberId, nameof(memberId));
    }

    public Guid EventId { get; }
    public string TenantId { get; }
    public DateTimeOffset OccurredAtUtc { get; }
    public Guid MemberId { get; }
    public string EventName => "member-enabled";
    public int Version => 1;
}
