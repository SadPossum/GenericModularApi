namespace Catalog.Application.Commands;

using Catalog.Contracts;
using Shared.Cqrs;

public sealed record UpdateCatalogItemCommand(
    Guid ItemId,
    string Sku,
    string Name,
    decimal Price,
    string Currency,
    IReadOnlyCollection<string>? AvailableRegions = null)
    : ITransactionalCommand<CatalogItemDto>;
