namespace Catalog.Application.Handlers;

using Catalog.Application.Commands;
using Catalog.Application.Mapping;
using Catalog.Application.Ports;
using Catalog.Contracts;
using Catalog.Domain.Aggregates;
using Catalog.Domain.Errors;
using Shared.Application.Caching;
using Shared.Application.Cqrs;
using Shared.Application.Identity;
using Shared.Application.Time;
using Shared.ErrorHandling;

internal sealed class UpdateCatalogItemCommandHandler(
    ICatalogItemRepository repository,
    ISystemClock clock,
    IIdGenerator idGenerator,
    ICacheInvalidationQueue cacheInvalidation)
    : ICommandHandler<UpdateCatalogItemCommand, CatalogItemDto>
{
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

        return Result.Success(CatalogItemMapper.ToDto(item));
    }
}
