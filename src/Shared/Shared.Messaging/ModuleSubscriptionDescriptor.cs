namespace Shared.Messaging;

using Shared.Modules;

public sealed record ModuleSubscriptionDescriptor
{
    public ModuleSubscriptionDescriptor(
        string producerModule,
        string eventType,
        string subject,
        string handlerName,
        bool tenantScoped)
    {
        this.ProducerModule = ModuleMetadataNaming.NormalizeModuleName(producerModule, nameof(producerModule));
        this.EventType = IntegrationEventNaming.NormalizeEventName(eventType, nameof(eventType));
        IntegrationEventSubject parsedSubject = IntegrationEventNaming.ParseSubject(subject, nameof(subject));
        this.SubjectPrefix = parsedSubject.SubjectPrefix;
        this.Subject = parsedSubject.CreateSubject();
        this.HandlerName = IntegrationEventNaming.NormalizeHandlerName(handlerName, nameof(handlerName));
        this.TenantScoped = tenantScoped;

        string expectedSubject = IntegrationEventNaming.CreateSubject(
            this.SubjectPrefix,
            this.ProducerModule,
            this.EventType,
            parsedSubject.Version);
        if (!string.Equals(this.Subject, expectedSubject, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Subscription subject must match the producer module and event type.",
                nameof(subject));
        }
    }

    public string ProducerModule { get; }
    public string EventType { get; }
    public string SubjectPrefix { get; }
    public string Subject { get; }
    public string HandlerName { get; }
    public bool TenantScoped { get; }
    public int Version => IntegrationEventNaming.ParseSubject(this.Subject).Version;

    public string CreateSubject(string subjectPrefix) =>
        IntegrationEventNaming.CreateSubject(subjectPrefix, this.ProducerModule, this.EventType, this.Version);
}
