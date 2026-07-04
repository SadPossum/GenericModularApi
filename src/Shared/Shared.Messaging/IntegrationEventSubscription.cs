namespace Shared.Messaging;

public sealed class IntegrationEventSubscription
{
    private IntegrationEventSubscription(
        string consumerModule,
        string producerModule,
        string eventName,
        int version,
        string subjectPrefix,
        Type eventType,
        Type handlerType,
        string handlerName,
        bool tenantScoped)
    {
        this.ConsumerModule = consumerModule;
        this.ProducerModule = producerModule;
        this.EventName = eventName;
        this.Version = version;
        this.SubjectPrefix = subjectPrefix;
        this.EventType = eventType;
        this.HandlerType = handlerType;
        this.HandlerName = handlerName;
        this.TenantScoped = tenantScoped;
    }

    public string ConsumerModule { get; }
    public string ProducerModule { get; }
    public string EventName { get; }
    public int Version { get; }
    public string SubjectPrefix { get; }
    public string Subject => this.CreateSubject(this.SubjectPrefix);
    public Type EventType { get; }
    public Type HandlerType { get; }
    public string HandlerName { get; }
    public bool TenantScoped { get; }

    public string CreateSubject(string subjectPrefix) =>
        IntegrationEventNaming.CreateSubject(subjectPrefix, this.ProducerModule, this.EventName, this.Version);

    public static IntegrationEventSubscription Create<TEvent, THandler>(
        string consumerModule,
        string subject,
        string handlerName,
        bool tenantScoped = true)
        where TEvent : IIntegrationEvent
        where THandler : class, IIntegrationEventHandler<TEvent>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(consumerModule);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(handlerName);

        IntegrationEventSubject parsedSubject = IntegrationEventNaming.ParseSubject(subject);
        return Create<TEvent, THandler>(
            consumerModule,
            parsedSubject.ModuleName,
            parsedSubject.EventName,
            parsedSubject.Version,
            handlerName,
            tenantScoped,
            parsedSubject.SubjectPrefix);
    }

    public static IntegrationEventSubscription Create<TEvent, THandler>(
        string consumerModule,
        string producerModule,
        string eventName,
        int version,
        string handlerName,
        bool tenantScoped = true,
        string subjectPrefix = IntegrationEventNaming.DefaultSubjectPrefix)
        where TEvent : IIntegrationEvent
        where THandler : class, IIntegrationEventHandler<TEvent>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(consumerModule);
        ArgumentException.ThrowIfNullOrWhiteSpace(producerModule);
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
        ArgumentException.ThrowIfNullOrWhiteSpace(handlerName);
        ArgumentOutOfRangeException.ThrowIfLessThan(version, 1);

        return new(
            IntegrationEventNaming.NormalizeModuleName(consumerModule),
            IntegrationEventNaming.NormalizeModuleName(producerModule),
            IntegrationEventNaming.NormalizeEventName(eventName),
            version,
            IntegrationEventNaming.NormalizeSubjectPrefix(subjectPrefix),
            typeof(TEvent),
            typeof(THandler),
            IntegrationEventNaming.NormalizeHandlerName(handlerName),
            tenantScoped);
    }
}
