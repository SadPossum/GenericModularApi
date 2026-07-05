namespace Notifications.Persistence.Repositories;

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Notifications.Application.Ports;
using Notifications.Contracts;
using Notifications.Domain.Aggregates;
using Notifications.Domain.ValueObjects;
using Shared.Pagination;
using ContractSeverity = Notifications.Contracts.NotificationSeverity;
using DomainSeverity = Notifications.Domain.ValueObjects.NotificationSeverity;

internal sealed class NotificationHistoryRepository(NotificationsDbContext dbContext) : INotificationHistoryRepository
{
    public async Task AddAsync(UserNotification notification, CancellationToken cancellationToken)
    {
        await dbContext.UserNotifications.AddAsync(notification, cancellationToken).ConfigureAwait(false);
    }

    public Task<bool> ExistsAsync(Guid notificationId, CancellationToken cancellationToken) =>
        dbContext.UserNotifications.AnyAsync(notification => notification.Id == notificationId, cancellationToken);

    public async Task<NotificationHistoryItem?> GetAsync(
        Guid notificationId,
        string userId,
        CancellationToken cancellationToken)
    {
        NotificationRecipient recipient = NormalizeRecipient(userId);
        UserNotification? notification = await dbContext.UserNotifications
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.Id == notificationId && item.Recipient == recipient,
                cancellationToken)
            .ConfigureAwait(false);

        return notification is null ? null : Map(notification);
    }

    public async Task<AdminNotificationHistoryItem?> GetTenantAsync(
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        UserNotification? notification = await dbContext.UserNotifications
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.Id == notificationId,
                cancellationToken)
            .ConfigureAwait(false);

        return notification is null ? null : MapAdmin(notification);
    }

    public async Task<NotificationHistoryListResponse> ListAsync(
        string userId,
        bool unreadOnly,
        PageRequest pageRequest,
        CancellationToken cancellationToken)
    {
        NotificationRecipient recipient = NormalizeRecipient(userId);
        IQueryable<UserNotification> userNotifications = dbContext.UserNotifications
            .AsNoTracking()
            .Where(notification => notification.Recipient == recipient);

        int unreadCount = await userNotifications
            .CountAsync(notification => notification.ReadAtUtc == null, cancellationToken)
            .ConfigureAwait(false);

        IQueryable<UserNotification> filtered = unreadOnly
            ? userNotifications.Where(notification => notification.ReadAtUtc == null)
            : userNotifications;

        int totalCount = await filtered.CountAsync(cancellationToken).ConfigureAwait(false);
        UserNotification[] items = await filtered
            .OrderByDescending(notification => notification.OccurredAtUtc)
            .ThenByDescending(notification => notification.CreatedAtUtc)
            .ThenBy(notification => notification.Id)
            .Skip(pageRequest.SkipCount)
            .Take(pageRequest.PageSize)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new NotificationHistoryListResponse(
            items.Select(Map).ToArray(),
            pageRequest.Page,
            pageRequest.PageSize,
            totalCount,
            unreadCount);
    }

    public async Task<AdminNotificationHistoryListResponse> ListTenantAsync(
        string? userId,
        bool unreadOnly,
        PageRequest pageRequest,
        CancellationToken cancellationToken)
    {
        IQueryable<UserNotification> notifications = dbContext.UserNotifications.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(userId))
        {
            NotificationRecipient recipient = NormalizeRecipient(userId);
            notifications = notifications.Where(notification => notification.Recipient == recipient);
        }

        int unreadCount = await notifications
            .CountAsync(notification => notification.ReadAtUtc == null, cancellationToken)
            .ConfigureAwait(false);

        IQueryable<UserNotification> filtered = unreadOnly
            ? notifications.Where(notification => notification.ReadAtUtc == null)
            : notifications;

        int totalCount = await filtered.CountAsync(cancellationToken).ConfigureAwait(false);
        UserNotification[] items = await filtered
            .OrderByDescending(notification => notification.CreatedAtUtc)
            .ThenBy(notification => notification.Id)
            .Skip(pageRequest.SkipCount)
            .Take(pageRequest.PageSize)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new AdminNotificationHistoryListResponse(
            items.Select(MapAdmin).ToArray(),
            pageRequest.Page,
            pageRequest.PageSize,
            totalCount,
            unreadCount);
    }

    public async Task<long> GetCurrentStreamSequenceForUserAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        NotificationRecipient recipient = NormalizeRecipient(userId);
        long? cursor = await dbContext.UserNotifications
            .AsNoTracking()
            .Where(notification => notification.Recipient == recipient)
            .MaxAsync(notification => (long?)notification.StreamSequence, cancellationToken)
            .ConfigureAwait(false);

        return cursor ?? 0;
    }

    public async Task<long> GetCurrentStreamSequenceForTenantAsync(
        string? userId,
        CancellationToken cancellationToken)
    {
        IQueryable<UserNotification> query = dbContext.UserNotifications.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            NotificationRecipient recipient = NormalizeRecipient(userId);
            query = query.Where(notification => notification.Recipient == recipient);
        }

        long? cursor = await query
            .MaxAsync(notification => (long?)notification.StreamSequence, cancellationToken)
            .ConfigureAwait(false);

        return cursor ?? 0;
    }

    public async Task<IReadOnlyList<NotificationHistoryItem>> ListNewForUserAsync(
        string userId,
        long afterStreamSequence,
        int batchSize,
        CancellationToken cancellationToken)
    {
        NotificationRecipient recipient = NormalizeRecipient(userId);
        UserNotification[] items = await dbContext.UserNotifications
            .AsNoTracking()
            .Where(notification =>
                notification.Recipient == recipient &&
                notification.StreamSequence > afterStreamSequence)
            .OrderBy(notification => notification.StreamSequence)
            .Take(batchSize)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return items.Select(Map).ToArray();
    }

    public async Task<IReadOnlyList<AdminNotificationHistoryItem>> ListNewForTenantAsync(
        string? userId,
        long afterStreamSequence,
        int batchSize,
        CancellationToken cancellationToken)
    {
        IQueryable<UserNotification> query = dbContext.UserNotifications.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            NotificationRecipient recipient = NormalizeRecipient(userId);
            query = query.Where(notification => notification.Recipient == recipient);
        }

        UserNotification[] items = await query
            .Where(notification => notification.StreamSequence > afterStreamSequence)
            .OrderBy(notification => notification.StreamSequence)
            .Take(batchSize)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return items.Select(MapAdmin).ToArray();
    }

    public async Task<bool> MarkReadAsync(
        Guid notificationId,
        string userId,
        DateTimeOffset readAtUtc,
        CancellationToken cancellationToken)
    {
        NotificationRecipient recipient = NormalizeRecipient(userId);
        UserNotification? notification = await dbContext.UserNotifications
            .FirstOrDefaultAsync(
                item => item.Id == notificationId && item.Recipient == recipient,
                cancellationToken)
            .ConfigureAwait(false);

        return notification is not null && notification.MarkRead(readAtUtc);
    }

    public async Task<int> MarkAllReadAsync(
        string userId,
        DateTimeOffset readAtUtc,
        CancellationToken cancellationToken)
    {
        NotificationRecipient recipient = NormalizeRecipient(userId);
        if (dbContext.Database.IsRelational())
        {
            return await dbContext.UserNotifications
                .Where(notification => notification.Recipient == recipient && notification.ReadAtUtc == null)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(notification => notification.ReadAtUtc, readAtUtc),
                    cancellationToken)
                .ConfigureAwait(false);
        }

        UserNotification[] unreadNotifications = await dbContext.UserNotifications
            .Where(notification => notification.Recipient == recipient && notification.ReadAtUtc == null)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        int updatedCount = 0;
        foreach (UserNotification notification in unreadNotifications)
        {
            if (notification.MarkRead(readAtUtc))
            {
                updatedCount++;
            }
        }

        return updatedCount;
    }

    private static NotificationHistoryItem Map(UserNotification notification)
    {
        using JsonDocument document = JsonDocument.Parse(notification.Payload.Json);
        return new NotificationHistoryItem(
            notification.Id,
            notification.Source.Module,
            notification.Source.Name,
            notification.Source.Version,
            notification.Content.Title,
            notification.Content.Body,
            ToContractSeverity(notification.Severity),
            notification.StreamSequence,
            notification.OccurredAtUtc,
            notification.CreatedAtUtc,
            notification.ReadAtUtc,
            document.RootElement.Clone());
    }

    private static AdminNotificationHistoryItem MapAdmin(UserNotification notification)
    {
        using JsonDocument document = JsonDocument.Parse(notification.Payload.Json);
        return new AdminNotificationHistoryItem(
            notification.Id,
            notification.TenantId,
            notification.Recipient.UserId,
            notification.Source.Module,
            notification.Source.Name,
            notification.Source.Version,
            notification.Content.Title,
            notification.Content.Body,
            ToContractSeverity(notification.Severity),
            notification.StreamSequence,
            notification.OccurredAtUtc,
            notification.CreatedAtUtc,
            notification.ReadAtUtc,
            document.RootElement.Clone());
    }

    private static NotificationRecipient NormalizeRecipient(string userId) =>
        NotificationRecipient.Create(userId).Value;

    private static ContractSeverity ToContractSeverity(DomainSeverity severity) =>
        severity switch
        {
            DomainSeverity.Info => ContractSeverity.Info,
            DomainSeverity.Success => ContractSeverity.Success,
            DomainSeverity.Warning => ContractSeverity.Warning,
            DomainSeverity.Error => ContractSeverity.Error,
            _ => ContractSeverity.Unknown
        };
}
