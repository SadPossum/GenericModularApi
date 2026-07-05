namespace Notifications.Application.Commands;

using Notifications.Contracts;
using Shared.Cqrs;

public sealed record MarkNotificationBroadcastReadCommand(
    Guid BroadcastId,
    string? TenantId,
    NotificationBroadcastRecipientKind RecipientKind,
    string RecipientId) : ITransactionalCommand<Unit>;
