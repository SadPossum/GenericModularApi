namespace Catalog.Application.Ports;

using Catalog.Contracts;
using Shared.Application.Queries;

public interface ICatalogItemReadRepository
{
    Task<CatalogItemDto?> GetAsync(Guid itemId, CancellationToken cancellationToken);
    Task<CatalogItemListResponse> ListAsync(PageRequest pageRequest, CancellationToken cancellationToken);
}
