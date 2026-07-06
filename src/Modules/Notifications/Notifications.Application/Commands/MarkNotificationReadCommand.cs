namespace Notifications.Application.Commands;

using Shared.AccessControl;
using Shared.Cqrs;

public sealed record MarkNotificationReadCommand(Guid NotificationId, AccessSubject Subject) : ITransactionalCommand<Unit>;
