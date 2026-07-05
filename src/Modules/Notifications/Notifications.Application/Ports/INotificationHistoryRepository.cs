namespace Notifications.Application.Ports;

using Notifications.Contracts;
using Notifications.Domain.Aggregates;
using Shared.Pagination;

public interface INotificationHistoryRepository
{
    Task AddAsync(UserNotification notification, CancellationToken cancellationToken);

    Task<NotificationHistoryItem?> GetAsync(
        Guid notificationId,
        string userId,
        CancellationToken cancellationToken);

    Task<AdminNotificationHistoryItem?> GetTenantAsync(
        Guid notificationId,
        CancellationToken cancellationToken);

    Task<NotificationHistoryListResponse> ListAsync(
        string userId,
        bool unreadOnly,
        PageRequest pageRequest,
        CancellationToken cancellationToken);

    Task<AdminNotificationHistoryListResponse> ListTenantAsync(
        string? userId,
        bool unreadOnly,
        PageRequest pageRequest,
        CancellationToken cancellationToken);

    Task<long> GetCurrentStreamSequenceForUserAsync(
        string userId,
        CancellationToken cancellationToken);

    Task<long> GetCurrentStreamSequenceForTenantAsync(
        string? userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<NotificationHistoryItem>> ListNewForUserAsync(
        string userId,
        long afterStreamSequence,
        int batchSize,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminNotificationHistoryItem>> ListNewForTenantAsync(
        string? userId,
        long afterStreamSequence,
        int batchSize,
        CancellationToken cancellationToken);

    Task<bool> MarkReadAsync(
        Guid notificationId,
        string userId,
        DateTimeOffset readAtUtc,
        CancellationToken cancellationToken);

    Task<int> MarkAllReadAsync(
        string userId,
        DateTimeOffset readAtUtc,
        CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid notificationId, CancellationToken cancellationToken);
}
