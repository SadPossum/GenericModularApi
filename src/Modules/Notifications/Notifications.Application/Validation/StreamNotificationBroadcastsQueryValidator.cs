namespace Notifications.Application.Validation;

using Notifications.Application;
using Notifications.Application.Queries;
using Shared.Cqrs;

internal sealed class StreamNotificationBroadcastsQueryValidator : IQueryValidator<StreamNotificationBroadcastsQuery>
{
    public IEnumerable<string> Validate(StreamNotificationBroadcastsQuery query)
    {
        foreach (string failure in NotificationBroadcastValidation.ValidateTenantId(query.TenantId))
        {
            yield return failure;
        }

        foreach (string failure in NotificationBroadcastValidation.ValidateRecipient(
            query.RecipientKind,
            query.RecipientId))
        {
            yield return failure;
        }

        if (query.AfterStreamSequence < 0)
        {
            yield return "Notification broadcast stream cursor is invalid.";
        }

        if (query.BatchSize is <= 0 or > NotificationStreamOptions.MaxBatchSize)
        {
            yield return $"Notification broadcast stream batch size must be between 1 and {NotificationStreamOptions.MaxBatchSize}.";
        }
    }
}
