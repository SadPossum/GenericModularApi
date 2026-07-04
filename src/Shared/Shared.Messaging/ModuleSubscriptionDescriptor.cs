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
        this.Subject = IntegrationEventNaming.NormalizeSubject(subject, nameof(subject));
        this.HandlerName = IntegrationEventNaming.NormalizeHandlerName(handlerName, nameof(handlerName));
        this.TenantScoped = tenantScoped;

        string expectedSubject = IntegrationEventNaming.CreateSubject(
            "gma",
            this.ProducerModule,
            this.EventType,
            VersionFromSubject(this.Subject));
        if (!string.Equals(this.Subject, expectedSubject, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Subscription subject must match the producer module and event type.",
                nameof(subject));
        }
    }

    public string ProducerModule { get; }
    public string EventType { get; }
    public string Subject { get; }
    public string HandlerName { get; }
    public bool TenantScoped { get; }

    private static int VersionFromSubject(string subject)
    {
        string versionSegment = subject.Split('.')[3];
        return int.Parse(versionSegment[1..], System.Globalization.CultureInfo.InvariantCulture);
    }
}
