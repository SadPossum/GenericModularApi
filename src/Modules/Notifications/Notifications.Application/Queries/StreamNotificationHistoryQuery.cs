namespace Notifications.Application.Queries;

using Notifications.Contracts;
using Shared.Cqrs;

public sealed record StreamNotificationHistoryQuery(
    string UserId,
    long AfterStreamSequence,
    int BatchSize)
    : IQuery<IReadOnlyList<NotificationHistoryItem>>;
