namespace Ordering.Persistence;

using Microsoft.EntityFrameworkCore;
using Ordering.Contracts;
using Shared.ProjectionRebuild;

internal sealed class OrderingProjectionRebuildCheckpointStore(OrderingDbContext dbContext)
    : IProjectionRebuildCheckpointStore
{
    public string ModuleName => OrderingModuleMetadata.Name;

    public async Task<ProjectionRebuildCheckpoint?> GetAsync(
        ProjectionRebuildCheckpointKey key,
        CancellationToken cancellationToken)
    {
        EnsureOrderingKey(key);

        OrderingProjectionRebuildCheckpoint? checkpoint = await dbContext
            .Set<OrderingProjectionRebuildCheckpoint>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item =>
                    item.RunId == key.RunId &&
                    item.TenantId == key.TenantId &&
                    item.ProjectionName == key.ProjectionName,
                cancellationToken)
            .ConfigureAwait(false);

        return checkpoint?.ToCheckpoint();
    }

    public async Task SaveAsync(
        ProjectionRebuildCheckpointKey key,
        ProjectionRebuildCheckpoint checkpoint,
        CancellationToken cancellationToken)
    {
        EnsureOrderingKey(key);

        OrderingProjectionRebuildCheckpoint? state = await dbContext
            .Set<OrderingProjectionRebuildCheckpoint>()
            .SingleOrDefaultAsync(
                item =>
                    item.RunId == key.RunId &&
                    item.TenantId == key.TenantId &&
                    item.ProjectionName == key.ProjectionName,
                cancellationToken)
            .ConfigureAwait(false);

        if (state is null)
        {
            dbContext.Set<OrderingProjectionRebuildCheckpoint>()
                .Add(OrderingProjectionRebuildCheckpoint.Create(key, checkpoint));
        }
        else
        {
            state.Update(checkpoint);
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void EnsureOrderingKey(ProjectionRebuildCheckpointKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (!string.Equals(key.ModuleName, OrderingModuleMetadata.Name, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Ordering projection checkpoint store cannot handle module '{key.ModuleName}'.");
        }

        if (string.IsNullOrWhiteSpace(key.TenantId))
        {
            throw new InvalidOperationException("Ordering projection checkpoint store requires a tenant id.");
        }
    }
}
