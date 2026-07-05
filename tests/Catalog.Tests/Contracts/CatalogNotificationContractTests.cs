namespace Catalog.Tests;

using Catalog.Contracts;
using Notifications.Contracts;
using Shared.Messaging;
using Xunit;

[Trait("Category", "Unit")]
public sealed class CatalogNotificationContractTests
{
    [Fact]
    public void Catalog_contract_declares_user_notification_request_event_without_hidden_notifications_subscription()
    {
        ModuleIntegrationEventDescriptor publishedEvent = Assert.Single(
            CatalogModuleMetadata.Descriptor.GetPublishedEvents(),
            item => string.Equals(item.EventType, UserNotificationRequestedIntegrationEvent.EventType, StringComparison.Ordinal));

        Assert.Equal(
            NotificationsIntegrationSubjects.CreateUserNotificationRequested(CatalogModuleMetadata.Name),
            publishedEvent.Subject);
        Assert.Empty(NotificationsModuleMetadata.Descriptor.GetSubscriptions());
    }
}
