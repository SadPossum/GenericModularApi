namespace Notifications.Application.Queries;

using Notifications.Contracts;
using Shared.AccessControl;
using Shared.Cqrs;

public sealed record StreamNotificationHistoryQuery(
    AccessSubject Subject,
    long AfterStreamSequence,
    int BatchSize)
    : IQuery<IReadOnlyList<NotificationHistoryItem>>;
