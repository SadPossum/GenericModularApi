namespace Notifications.Application.Queries;

using Shared.Cqrs;

public sealed record GetTenantNotificationStreamCursorQuery(string? UserId)
    : IQuery<long>;
