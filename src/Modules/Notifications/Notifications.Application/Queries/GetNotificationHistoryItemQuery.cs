namespace Notifications.Application.Queries;

using Notifications.Contracts;
using Shared.Cqrs;

public sealed record GetNotificationHistoryItemQuery(Guid NotificationId, string UserId)
    : IQuery<NotificationHistoryItem>;
