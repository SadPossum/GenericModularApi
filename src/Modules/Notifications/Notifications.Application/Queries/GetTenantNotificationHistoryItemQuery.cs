namespace Notifications.Application.Queries;

using Notifications.Contracts;
using Shared.Cqrs;

public sealed record GetTenantNotificationHistoryItemQuery(Guid NotificationId)
    : IQuery<AdminNotificationHistoryItem>;
