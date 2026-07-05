namespace Notifications.Application.Queries;

using Notifications.Contracts;
using Shared.Cqrs;

public sealed record GetNotificationBroadcastQuery(
    Guid BroadcastId,
    string? TenantId,
    NotificationBroadcastRecipientKind RecipientKind,
    string RecipientId) : IQuery<NotificationBroadcastItem>;
