namespace Notifications.Application.Queries;

using Notifications.Contracts;
using Shared.Cqrs;

public sealed record ListNotificationBroadcastsQuery(
    string? TenantId,
    NotificationBroadcastRecipientKind RecipientKind,
    string RecipientId,
    bool UnreadOnly = false,
    int Page = Shared.Pagination.PageRequest.DefaultPage,
    int PageSize = Shared.Pagination.PageRequest.DefaultPageSize) : IQuery<NotificationBroadcastListResponse>;
