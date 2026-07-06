namespace Notifications.Application.Queries;

using Shared.AccessControl;
using Notifications.Contracts;
using Shared.Cqrs;

public sealed record GetNotificationHistoryItemQuery(Guid NotificationId, AccessSubject Subject)
    : IQuery<NotificationHistoryItem>;
