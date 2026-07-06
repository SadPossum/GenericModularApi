namespace Notifications.Application.Queries;

using Shared.AccessControl;
using Shared.Cqrs;

public sealed record GetNotificationStreamCursorQuery(AccessSubject Subject)
    : IQuery<long>;
