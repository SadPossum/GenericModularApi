namespace Notifications.Application.Commands;

using Notifications.Contracts;
using Shared.Cqrs;

public sealed record CreateNotificationBroadcastCommand(
    NotificationBroadcastAudience Audience,
    string? TenantId,
    string Module,
    string Name,
    int Version,
    string Title,
    string? Body,
    NotificationSeverity Severity,
    DateTimeOffset? OccurredAtUtc,
    string PayloadJson) : ITransactionalCommand<AdminCreateNotificationBroadcastResponse>;
