namespace Catalog.Application.Queries;

using Catalog.Contracts;
using Shared.Cqrs;

public sealed record GetCatalogItemQuery(Guid ItemId) : IQuery<CatalogItemDto>;
