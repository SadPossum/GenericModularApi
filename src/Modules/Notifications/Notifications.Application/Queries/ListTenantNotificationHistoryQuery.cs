namespace Notifications.Application.Queries;

using Notifications.Contracts;
using Shared.Cqrs;

public sealed record ListTenantNotificationHistoryQuery(
    string? UserId = null,
    bool UnreadOnly = false,
    int Page = Shared.Pagination.PageRequest.DefaultPage,
    int PageSize = Shared.Pagination.PageRequest.DefaultPageSize)
    : IQuery<AdminNotificationHistoryListResponse>;
