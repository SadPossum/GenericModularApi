namespace Auth.Contracts;

using Shared.Messaging;

public sealed record MemberRegisteredIntegrationEvent : IntegrationEvent
{
    public MemberRegisteredIntegrationEvent(
        Guid eventId,
        string tenantId,
        DateTimeOffset occurredAtUtc,
        Guid memberId,
        string username)
        : base(eventId, tenantId, occurredAtUtc, "member-registered", version: 1)
    {
        this.MemberId = IntegrationEventContractGuards.RequireId(memberId, nameof(memberId));
        this.Username = IntegrationEventContractGuards.NormalizeRequiredText(
            username,
            AuthContractLimits.UsernameMaxLength,
            nameof(username));
    }

    public Guid MemberId { get; }
    public string Username { get; }
}
