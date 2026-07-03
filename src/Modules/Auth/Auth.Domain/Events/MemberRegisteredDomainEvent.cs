namespace Auth.Domain.Events;

using Auth.Domain.Entities;
using Auth.Domain.ValueObjects;
using Shared.Domain;

public sealed record MemberRegisteredDomainEvent : IDomainEvent
{
    public MemberRegisteredDomainEvent(
        Guid eventId,
        DateTimeOffset occurredAtUtc,
        MemberId memberId,
        string tenantId,
        string username)
    {
        this.EventId = DomainEventGuards.RequireId(eventId, nameof(eventId));
        this.OccurredAtUtc = DomainEventGuards.RequireOccurredAtUtc(occurredAtUtc, nameof(occurredAtUtc));
        _ = DomainEventGuards.RequireId(memberId.Value, nameof(memberId));
        this.MemberId = memberId;
        this.TenantId = DomainEventGuards.NormalizeTenantId(tenantId, nameof(tenantId));
        this.Username = DomainEventGuards.NormalizeRequiredText(username, MemberUsername.ValueMaxLength, nameof(username));
    }

    public Guid EventId { get; }
    public DateTimeOffset OccurredAtUtc { get; }
    public MemberId MemberId { get; }
    public string TenantId { get; }
    public string Username { get; }
}
