namespace Notifications.Persistence;

using Microsoft.Extensions.Logging;
using Notifications.Application.Ports;
using Notifications.Domain.Aggregates;
using Notifications.Domain.Errors;
using Notifications.Domain.ValueObjects;
using Shared.Notifications;
using Shared.Results;
using Shared.Runtime.Time;
using DomainNotificationSeverity = Notifications.Domain.ValueObjects.NotificationSeverity;
using SharedNotificationSeverity = Shared.Notifications.NotificationSeverity;

internal sealed class NotificationHistoryWriter(
    INotificationHistoryRepository repository,
    NotificationsDbContext dbContext,
    ISystemClock clock,
    ILogger<NotificationHistoryWriter> logger)
    : IUserNotificationHistoryWriter
{
    public async ValueTask SaveAsync(UserNotificationMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (await repository.ExistsAsync(message.Id, cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        Result<DomainNotificationSeverity> severity = ToDomainSeverity(message.Severity);
        if (severity.IsFailure)
        {
            logger.LogWarning(
                "User notification {NotificationId} could not be converted to a history record. Error: {ErrorCode}.",
                message.Id,
                severity.Error.Code);
            return;
        }

        Result<UserNotification> notification = UserNotification.Create(
            message.Id,
            message.TenantId,
            message.UserId,
            message.Module,
            message.Name,
            message.Version,
            message.Title,
            message.Body,
            severity.Value,
            message.OccurredAtUtc,
            clock.UtcNow,
            message.Payload.GetRawText());

        if (notification.IsFailure)
        {
            logger.LogWarning(
                "User notification {NotificationId} could not be converted to a history record. Error: {ErrorCode}.",
                message.Id,
                notification.Error.Code);
            return;
        }

        await repository.AddAsync(notification.Value, cancellationToken).ConfigureAwait(false);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static Result<DomainNotificationSeverity> ToDomainSeverity(SharedNotificationSeverity severity) =>
        severity switch
        {
            SharedNotificationSeverity.Info => Result.Success(DomainNotificationSeverity.Info),
            SharedNotificationSeverity.Success => Result.Success(DomainNotificationSeverity.Success),
            SharedNotificationSeverity.Warning => Result.Success(DomainNotificationSeverity.Warning),
            SharedNotificationSeverity.Error => Result.Success(DomainNotificationSeverity.Error),
            _ => Result.Failure<DomainNotificationSeverity>(NotificationsDomainErrors.SeverityInvalid)
        };
}
