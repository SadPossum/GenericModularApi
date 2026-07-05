namespace Notifications.Application.Queries;

using Shared.Cqrs;

public sealed record GetNotificationStreamCursorQuery(string UserId)
    : IQuery<long>;
