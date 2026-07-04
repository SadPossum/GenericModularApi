namespace Shared.Messaging;

public sealed record ModuleIntegrationEventDescriptor
{
    public ModuleIntegrationEventDescriptor(string eventType, string subject, int version, bool tenantScoped)
    {
        this.EventType = IntegrationEventNaming.NormalizeEventName(eventType, nameof(eventType));
        ArgumentOutOfRangeException.ThrowIfLessThan(version, 1);
        IntegrationEventSubject parsedSubject = IntegrationEventNaming.ParseSubject(subject, nameof(subject));
        this.SubjectPrefix = parsedSubject.SubjectPrefix;
        this.ModuleName = parsedSubject.ModuleName;
        this.Subject = parsedSubject.CreateSubject();
        this.Version = version;
        this.TenantScoped = tenantScoped;
    }

    public string EventType { get; }
    public string SubjectPrefix { get; }
    public string ModuleName { get; }
    public string Subject { get; }
    public int Version { get; }
    public bool TenantScoped { get; }

    public string CreateSubject(string subjectPrefix) =>
        IntegrationEventNaming.CreateSubject(subjectPrefix, this.ModuleName, this.EventType, this.Version);
}
