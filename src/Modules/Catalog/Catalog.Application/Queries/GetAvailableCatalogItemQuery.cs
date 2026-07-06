namespace Catalog.Application.Queries;

using Catalog.Contracts;
using Shared.AccessControl;
using Shared.Cqrs;

public sealed record GetAvailableCatalogItemQuery(
    Guid ItemId,
    AccessSubject Subject,
    string RegionCode,
    string? SubjectRegionCode)
    : IQuery<CatalogItemDto>;
