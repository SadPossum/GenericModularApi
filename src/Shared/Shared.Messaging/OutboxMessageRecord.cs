namespace Shared.Messaging;

using Shared.Naming;

public sealed record OutboxMessageRecord
{
    public const int SubjectMaxLength = IntegrationEventEnvelope.SubjectMaxLength;
    public const int EventTypeMaxLength = IntegrationEventEnvelope.EventTypeMaxLength;

    public OutboxMessageRecord(
        Guid id,
        string subject,
        string eventType,
        int version,
        string tenantId,
        DateTimeOffset occurredAtUtc,
        string payload)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("id must not be empty.", nameof(id));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(version, 1);

        if (occurredAtUtc == default)
        {
            throw new ArgumentException("occurredAtUtc must not be the default timestamp.", nameof(occurredAtUtc));
        }

        this.Id = id;
        this.Subject = IntegrationEventEnvelope.NormalizeSubject(subject);
        this.EventType = IntegrationEventEnvelope.NormalizeRequired(eventType, EventTypeMaxLength, nameof(eventType));
        this.Version = version;
        this.TenantId = TenantIds.Normalize(tenantId);
        this.OccurredAtUtc = occurredAtUtc;
        this.Payload = IntegrationEventEnvelope.NormalizeRequired(payload, int.MaxValue, nameof(payload), allowControlCharacters: true);
    }

    public Guid Id { get; }
    public string Subject { get; }
    public string EventType { get; }
    public int Version { get; }
    public string TenantId { get; }
    public DateTimeOffset OccurredAtUtc { get; }
    public string Payload { get; }
}
