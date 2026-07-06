namespace Catalog.Application.Handlers;

using System.Text.Json;
using Catalog.Application.Commands;
using Catalog.Application.Mapping;
using Catalog.Application.Ports;
using Catalog.Contracts;
using Catalog.Domain.Aggregates;
using Catalog.Domain.Errors;
using Notifications.Contracts;
using Shared.Caching;
using Shared.Cqrs;
using Shared.Messaging;
using Shared.Runtime.Identity;
using Shared.Runtime.Time;
using Shared.Results;

internal sealed class UpdateCatalogItemCommandHandler(
    ICatalogItemRepository repository,
    ISystemClock clock,
    IIdGenerator idGenerator,
    ICacheInvalidationQueue cacheInvalidation,
    IOutboxWriterRegistry outboxWriters)
    : ICommandHandler<UpdateCatalogItemCommand, CatalogItemDto>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<CatalogItemDto>> HandleAsync(UpdateCatalogItemCommand command, CancellationToken cancellationToken)
    {
        CatalogItem? item = await repository.GetAsync(command.ItemId, cancellationToken).ConfigureAwait(false);
        if (item is null)
        {
            return Result.Failure<CatalogItemDto>(CatalogDomainErrors.ItemNotFound);
        }

        string normalizedSku = CatalogItem.NormalizeSku(command.Sku);
        if (await repository.SkuExistsAsync(normalizedSku, item.Id, cancellationToken).ConfigureAwait(false))
        {
            return Result.Failure<CatalogItemDto>(CatalogDomainErrors.SkuAlreadyExists);
        }

        Result result = item.Update(
            command.Sku,
            command.Name,
            command.Price,
            command.Currency,
            idGenerator.NewId(),
            clock.UtcNow);

        if (result.IsFailure)
        {
            return Result.Failure<CatalogItemDto>(result.Error);
        }

        cacheInvalidation.Remove(CatalogCache.Item(item.Id));
        cacheInvalidation.RemoveByTag(CatalogCache.ItemsTag());

        CatalogItemDto dto = CatalogItemMapper.ToDto(item);
        await this.EnqueueNotificationRequestAsync(command.NotificationUserId, item.TenantId, dto, cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(dto);
    }

    private async ValueTask EnqueueNotificationRequestAsync(
        string? notificationUserId,
        string tenantId,
        CatalogItemDto item,
        CancellationToken cancellationToken)
    {
        if (!NotificationRecipientUserIds.TryNormalize(notificationUserId, out string normalizedUserId))
        {
            return;
        }

        string payloadJson = JsonSerializer.Serialize(
            new CatalogItemUpdatedNotificationPayload(item.ItemId, item.Sku, item.Name, item.Status),
            JsonOptions);

        await outboxWriters.GetRequired(CatalogModuleMetadata.Name).EnqueueAsync(
                new UserNotificationRequestedIntegrationEvent(
                    idGenerator.NewId(),
                    tenantId,
                    clock.UtcNow,
                    normalizedUserId,
                    CatalogModuleMetadata.Name,
                    CatalogNotificationNames.ItemUpdated,
                    CatalogNotificationNames.ItemUpdatedVersion,
                    "Catalog item updated",
                    $"Item {item.Sku} was updated.",
                    NotificationSeverity.Info,
                    payloadJson),
                cancellationToken)
            .ConfigureAwait(false);
    }

    private sealed record CatalogItemUpdatedNotificationPayload(
        Guid ItemId,
        string Sku,
        string Name,
        CatalogItemStatus Status);
}
