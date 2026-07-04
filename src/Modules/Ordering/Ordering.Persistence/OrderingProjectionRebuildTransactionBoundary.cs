namespace Ordering.Persistence;

using Ordering.Contracts;
using Shared.ProjectionRebuild.EntityFrameworkCore;

internal sealed class OrderingProjectionRebuildTransactionBoundary(OrderingDbContext dbContext)
    : EfProjectionRebuildTransactionBoundary<OrderingDbContext>(dbContext, OrderingModuleMetadata.Name);
