namespace Notifications.Application.Commands;

using Notifications.Contracts;
using Shared.AccessControl;
using Shared.Cqrs;

public sealed record MarkAllNotificationsReadCommand(AccessSubject Subject)
    : ITransactionalCommand<MarkAllNotificationsReadResponse>;
