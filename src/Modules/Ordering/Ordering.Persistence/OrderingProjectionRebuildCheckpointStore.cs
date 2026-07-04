namespace Ordering.Persistence;

using Ordering.Contracts;
using Shared.ProjectionRebuild;
using Shared.ProjectionRebuild.EntityFrameworkCore;

internal sealed class OrderingProjectionRebuildCheckpointStore(OrderingDbContext dbContext)
    : EfProjectionRebuildCheckpointStore<OrderingDbContext, OrderingProjectionRebuildCheckpoint>(
        dbContext,
        OrderingModuleMetadata.Name,
        tenantScoped: true,
        OrderingProjectionRebuildCheckpoint.CreateEmpty);
