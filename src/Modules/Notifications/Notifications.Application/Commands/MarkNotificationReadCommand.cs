namespace Notifications.Application.Commands;

using Shared.Cqrs;

public sealed record MarkNotificationReadCommand(Guid NotificationId, string UserId) : ITransactionalCommand<Unit>;
