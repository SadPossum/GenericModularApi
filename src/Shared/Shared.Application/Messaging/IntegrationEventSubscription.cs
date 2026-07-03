namespace Shared.Application.Messaging;

public sealed class IntegrationEventSubscription
{
    private IntegrationEventSubscription(
        string consumerModule,
        string subject,
        Type eventType,
        Type handlerType,
        string handlerName,
        bool tenantScoped)
    {
        this.ConsumerModule = consumerModule;
        this.Subject = subject;
        this.EventType = eventType;
        this.HandlerType = handlerType;
        this.HandlerName = handlerName;
        this.TenantScoped = tenantScoped;
    }

    public string ConsumerModule { get; }
    public string Subject { get; }
    public Type EventType { get; }
    public Type HandlerType { get; }
    public string HandlerName { get; }
    public bool TenantScoped { get; }

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

        return new(
            IntegrationEventNaming.NormalizeModuleName(consumerModule),
            IntegrationEventNaming.NormalizeSubject(subject),
            typeof(TEvent),
            typeof(THandler),
            IntegrationEventNaming.NormalizeHandlerName(handlerName),
            tenantScoped);
    }
}
