namespace Notifications.Application.Queries;

using Notifications.Contracts;
using Shared.Cqrs;

public sealed record GetNotificationBroadcastStreamCursorQuery(
    string? TenantId,
    NotificationBroadcastRecipientKind RecipientKind,
    string RecipientId) : IQuery<long>;
