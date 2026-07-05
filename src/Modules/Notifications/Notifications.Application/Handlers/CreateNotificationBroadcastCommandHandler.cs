namespace Notifications.Application.Handlers;

using Notifications.Application.Commands;
using Notifications.Application.Ports;
using Notifications.Contracts;
using Notifications.Domain.Aggregates;
using Notifications.Domain.ValueObjects;
using Shared.Cqrs;
using Shared.Results;
using Shared.Runtime.Identity;
using Shared.Runtime.Time;

internal sealed class CreateNotificationBroadcastCommandHandler(
    INotificationBroadcastRepository repository,
    IIdGenerator idGenerator,
    ISystemClock clock)
    : ICommandHandler<CreateNotificationBroadcastCommand, AdminCreateNotificationBroadcastResponse>
{
    public async Task<Result<AdminCreateNotificationBroadcastResponse>> HandleAsync(
        CreateNotificationBroadcastCommand command,
        CancellationToken cancellationToken)
    {
        DateTimeOffset createdAtUtc = clock.UtcNow;
        Guid broadcastId = idGenerator.NewId();

        Result<NotificationBroadcast> broadcast = NotificationBroadcast.Create(
            broadcastId,
            command.TenantId,
            NotificationBroadcastAudienceMapper.ToDomainValue(command.Audience),
            command.Module,
            command.Name,
            command.Version,
            command.Title,
            command.Body,
            NotificationSeverityMapper.ToDomainValue(command.Severity),
            command.OccurredAtUtc ?? createdAtUtc,
            createdAtUtc,
            command.PayloadJson);

        if (broadcast.IsFailure)
        {
            return Result.Failure<AdminCreateNotificationBroadcastResponse>(broadcast.Error);
        }

        await repository.AddAsync(broadcast.Value, cancellationToken).ConfigureAwait(false);
        return Result.Success(new AdminCreateNotificationBroadcastResponse(broadcastId));
    }
}
