namespace Ordering.Domain.Visibility;

using Shared.Results;

public static class OrderingVisibilityPolicy
{
    public static Result<UserOrdersScope> CanViewOwnOrders(OrderViewer viewer) =>
        Result.Success(new UserOrdersScope(viewer.TenantId, viewer.UserId));
}
