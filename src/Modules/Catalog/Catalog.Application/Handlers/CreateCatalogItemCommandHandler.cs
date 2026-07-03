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
using Shared.Application.Tenancy;
using Shared.Application.Time;
using Shared.ErrorHandling;

internal sealed class CreateCatalogItemCommandHandler(
    ICatalogItemRepository repository,
    ITenantContext tenantContext,
    ISystemClock clock,
    IIdGenerator idGenerator,
    ICacheInvalidationQueue cacheInvalidation)
    : ICommandHandler<CreateCatalogItemCommand, CatalogItemDto>
{
    public async Task<Result<CatalogItemDto>> HandleAsync(CreateCatalogItemCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tenantContext.TenantId))
        {
            return Result.Failure<CatalogItemDto>(CatalogDomainErrors.TenantRequired);
        }

        Result<CatalogItem> itemResult = CatalogItem.Create(
            idGenerator.NewId(),
            tenantContext.TenantId,
            command.Sku,
            command.Name,
            command.Price,
            command.Currency,
            idGenerator.NewId(),
            clock.UtcNow);

        if (itemResult.IsFailure)
        {
            return Result.Failure<CatalogItemDto>(itemResult.Error);
        }

        CatalogItem item = itemResult.Value;
        if (await repository.SkuExistsAsync(item.Sku, excludingItemId: null, cancellationToken).ConfigureAwait(false))
        {
            return Result.Failure<CatalogItemDto>(CatalogDomainErrors.SkuAlreadyExists);
        }

        await repository.AddAsync(item, cancellationToken).ConfigureAwait(false);
        cacheInvalidation.RemoveByTag(CatalogCache.ItemsTag());

        return Result.Success(CatalogItemMapper.ToDto(item));
    }
}
