namespace Notifications.Application.Queries;

using Notifications.Contracts;
using Shared.Cqrs;

public sealed record ListPlatformNotificationBroadcastsQuery(
    int Page = Shared.Pagination.PageRequest.DefaultPage,
    int PageSize = Shared.Pagination.PageRequest.DefaultPageSize) : IQuery<AdminNotificationBroadcastListResponse>;
