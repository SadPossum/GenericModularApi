namespace Notifications.Application.Queries;

using Notifications.Contracts;
using Shared.Cqrs;

public sealed record StreamNotificationBroadcastsQuery(
    string? TenantId,
    NotificationBroadcastRecipientKind RecipientKind,
    string RecipientId,
    long AfterStreamSequence,
    int BatchSize) : IQuery<IReadOnlyList<NotificationBroadcastItem>>;
