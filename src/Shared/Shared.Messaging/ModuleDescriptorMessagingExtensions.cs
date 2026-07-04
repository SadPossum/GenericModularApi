namespace Shared.Messaging;

using Shared.Modules;

public static class ModuleDescriptorMessagingExtensions
{
    public static ModuleDescriptorBuilder WithPublishedEvent(
        this ModuleDescriptorBuilder builder,
        ModuleIntegrationEventDescriptor publishedEvent)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(publishedEvent);
        return builder.WithPublishedEvents([publishedEvent]);
    }

    public static ModuleDescriptorBuilder WithPublishedEvents(
        this ModuleDescriptorBuilder builder,
        IReadOnlyList<ModuleIntegrationEventDescriptor> publishedEvents)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.WithFeature(
            new ModulePublishedEventsDescriptor(publishedEvents),
            static (existing, incoming) =>
            {
                return new ModulePublishedEventsDescriptor(existing
                    .PublishedEvents
                    .Concat(incoming.PublishedEvents)
                    .ToArray());
            });
    }

    public static ModuleDescriptorBuilder WithSubscription(
        this ModuleDescriptorBuilder builder,
        ModuleSubscriptionDescriptor subscription)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(subscription);
        return builder.WithSubscriptions([subscription]);
    }

    public static ModuleDescriptorBuilder WithSubscriptions(
        this ModuleDescriptorBuilder builder,
        IReadOnlyList<ModuleSubscriptionDescriptor> subscriptions)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.WithFeature(
            new ModuleSubscriptionsDescriptor(subscriptions),
            static (existing, incoming) =>
            {
                return new ModuleSubscriptionsDescriptor(existing
                    .Subscriptions
                    .Concat(incoming.Subscriptions)
                    .ToArray());
            });
    }

    public static IReadOnlyList<ModuleIntegrationEventDescriptor> GetPublishedEvents(this ModuleDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        return descriptor.GetFeature<ModulePublishedEventsDescriptor>()?.PublishedEvents ?? [];
    }

    public static IReadOnlyList<ModuleSubscriptionDescriptor> GetSubscriptions(this ModuleDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        return descriptor.GetFeature<ModuleSubscriptionsDescriptor>()?.Subscriptions ?? [];
    }
}
