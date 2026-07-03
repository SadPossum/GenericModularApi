namespace Catalog.Application.Commands;

using Catalog.Contracts;
using Shared.Application.Cqrs;

public sealed record CreateCatalogItemCommand(string Sku, string Name, decimal Price, string Currency)
    : ITransactionalCommand<CatalogItemDto>;
