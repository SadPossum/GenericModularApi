namespace Ordering.Application.Ports;

using Ordering.Contracts;
using Shared.Pagination;

public interface IOrderReadRepository
{
    Task<OrderDto?> GetAsync(Guid orderId, CancellationToken cancellationToken);

    Task<OrderListResponse> ListAsync(PageRequest pageRequest, CancellationToken cancellationToken);
}
