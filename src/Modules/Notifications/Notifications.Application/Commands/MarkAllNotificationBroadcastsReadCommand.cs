namespace Notifications.Application.Commands;

using Notifications.Contracts;
using Shared.Cqrs;

public sealed record MarkAllNotificationBroadcastsReadCommand(
    string? TenantId,
    NotificationBroadcastRecipientKind RecipientKind,
    string RecipientId) : ITransactionalCommand<MarkAllNotificationBroadcastsReadResponse>;
