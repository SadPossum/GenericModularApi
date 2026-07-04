namespace Shared.Messaging;

public interface IIntegrationEvent
{
    Guid EventId { get; }
    string TenantId { get; }
    DateTimeOffset OccurredAtUtc { get; }
    string EventName { get; }
    int Version { get; }
}
