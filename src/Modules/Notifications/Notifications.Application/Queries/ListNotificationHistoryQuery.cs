namespace Notifications.Application.Queries;

using Notifications.Contracts;
using Shared.Cqrs;
using Shared.Pagination;

public sealed record ListNotificationHistoryQuery(
    string UserId,
    bool UnreadOnly = false,
    int Page = PageRequest.DefaultPage,
    int PageSize = PageRequest.DefaultPageSize)
    : IQuery<NotificationHistoryListResponse>;
