namespace Notifications.Application.Commands;

using Notifications.Contracts;
using Shared.Cqrs;

public sealed record MarkAllNotificationsReadCommand(string UserId)
    : ITransactionalCommand<MarkAllNotificationsReadResponse>;
