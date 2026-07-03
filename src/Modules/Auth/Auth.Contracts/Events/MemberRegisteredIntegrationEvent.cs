namespace Auth.Contracts;

using Shared.Application.Messaging;

public sealed record MemberRegisteredIntegrationEvent : IIntegrationEvent
{
    public MemberRegisteredIntegrationEvent(
        Guid eventId,
        string tenantId,
        DateTimeOffset occurredAtUtc,
        Guid memberId,
        string username)
    {
        this.EventId = IntegrationEventContractGuards.RequireId(eventId, nameof(eventId));
        this.TenantId = IntegrationEventContractGuards.NormalizeTenantId(tenantId, nameof(tenantId));
        this.OccurredAtUtc = IntegrationEventContractGuards.RequireOccurredAtUtc(occurredAtUtc, nameof(occurredAtUtc));
        this.MemberId = IntegrationEventContractGuards.RequireId(memberId, nameof(memberId));
        this.Username = IntegrationEventContractGuards.NormalizeRequiredText(
            username,
            AuthContractLimits.UsernameMaxLength,
            nameof(username));
    }

    public Guid EventId { get; }
    public string TenantId { get; }
    public DateTimeOffset OccurredAtUtc { get; }
    public Guid MemberId { get; }
    public string Username { get; }
    public string EventName => "member-registered";
    public int Version => 1;
}
