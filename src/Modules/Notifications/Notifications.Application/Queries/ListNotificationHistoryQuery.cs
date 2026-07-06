namespace Notifications.Application.Queries;

using Notifications.Contracts;
using Shared.AccessControl;
using Shared.Cqrs;
using Shared.Pagination;

public sealed record ListNotificationHistoryQuery(
    AccessSubject Subject,
    bool UnreadOnly = false,
    int Page = PageRequest.DefaultPage,
    int PageSize = PageRequest.DefaultPageSize)
    : IQuery<NotificationHistoryListResponse>;
