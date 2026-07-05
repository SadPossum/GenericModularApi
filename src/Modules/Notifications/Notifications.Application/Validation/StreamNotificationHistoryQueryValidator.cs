namespace Notifications.Application.Validation;

using Notifications.Application;
using Notifications.Application.Queries;
using Notifications.Contracts;
using Shared.Cqrs;

internal sealed class StreamNotificationHistoryQueryValidator : IQueryValidator<StreamNotificationHistoryQuery>
{
    public IEnumerable<string> Validate(StreamNotificationHistoryQuery query)
    {
        if (!NotificationRecipientUserIds.TryNormalize(query.UserId, out _))
        {
            yield return "Notification user id is required.";
        }

        if (query.AfterStreamSequence < 0)
        {
            yield return "Notification stream cursor must be zero or greater.";
        }

        if (query.BatchSize is < 1 or > NotificationStreamOptions.MaxBatchSize)
        {
            yield return $"Notification stream batch size must be between 1 and {NotificationStreamOptions.MaxBatchSize}.";
        }
    }
}
