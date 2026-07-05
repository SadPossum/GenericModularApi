namespace Notifications.Application.Handlers;

using Notifications.Application.Ports;
using Notifications.Contracts;
using Notifications.Domain.Aggregates;
using Notifications.Domain.ValueObjects;
using Shared.Messaging;
using Shared.Runtime.Time;
using ContractNotificationSeverity = Notifications.Contracts.NotificationSeverity;
using DomainNotificationSeverity = Notifications.Domain.ValueObjects.NotificationSeverity;

[IntegrationEventHandler("user-notification-request", RequiresExplicitProducerBinding = true)]
internal sealed class UserNotificationRequestedIntegrationEventHandler(
    INotificationHistoryRepository repository,
    ISystemClock clock)
    : IIntegrationEventHandler<UserNotificationRequestedIntegrationEvent>
{
    public async Task HandleAsync(
        UserNotificationRequestedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        if (await repository.ExistsAsync(integrationEvent.EventId, cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        Shared.Results.Result<UserNotification> notification = UserNotification.Create(
            integrationEvent.EventId,
            integrationEvent.TenantId,
            integrationEvent.UserId,
            integrationEvent.SourceModule,
            integrationEvent.NotificationName,
            integrationEvent.NotificationVersion,
            integrationEvent.Title,
            integrationEvent.Body,
            ToDomainSeverity(integrationEvent.Severity),
            integrationEvent.OccurredAtUtc,
            clock.UtcNow,
            integrationEvent.PayloadJson);

        if (notification.IsFailure)
        {
            throw new InvalidOperationException(
                $"Notification request {integrationEvent.EventId} could not be projected: {notification.Error.Code}.");
        }

        await repository.AddAsync(notification.Value, cancellationToken).ConfigureAwait(false);
    }

    private static DomainNotificationSeverity ToDomainSeverity(ContractNotificationSeverity severity) =>
        severity switch
        {
            ContractNotificationSeverity.Info => DomainNotificationSeverity.Info,
            ContractNotificationSeverity.Success => DomainNotificationSeverity.Success,
            ContractNotificationSeverity.Warning => DomainNotificationSeverity.Warning,
            ContractNotificationSeverity.Error => DomainNotificationSeverity.Error,
            _ => throw new ArgumentOutOfRangeException(
                nameof(severity),
                severity,
                "Notification severity must be a defined non-unknown value.")
        };
}
