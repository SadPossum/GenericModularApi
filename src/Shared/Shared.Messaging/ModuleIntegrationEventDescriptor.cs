namespace Shared.Messaging;

public sealed record ModuleIntegrationEventDescriptor
{
    public ModuleIntegrationEventDescriptor(string eventType, string subject, int version, bool tenantScoped)
    {
        this.EventType = IntegrationEventNaming.NormalizeEventName(eventType, nameof(eventType));
        ArgumentOutOfRangeException.ThrowIfLessThan(version, 1);
        this.Subject = IntegrationEventNaming.NormalizeSubject(subject, nameof(subject));
        this.Version = version;
        this.TenantScoped = tenantScoped;
    }

    public string EventType { get; }
    public string Subject { get; }
    public int Version { get; }
    public bool TenantScoped { get; }
}
