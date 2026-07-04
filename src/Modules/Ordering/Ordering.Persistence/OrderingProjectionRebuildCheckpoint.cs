namespace Ordering.Persistence;

using Shared.ProjectionRebuild;

public sealed class OrderingProjectionRebuildCheckpoint : ProjectionRebuildCheckpointState
{
    private OrderingProjectionRebuildCheckpoint() { }

    private OrderingProjectionRebuildCheckpoint(
        ProjectionRebuildCheckpointKey key,
        ProjectionRebuildCheckpoint checkpoint)
        : base(key, checkpoint, tenantScoped: true)
    {
    }

    public static OrderingProjectionRebuildCheckpoint Create(
        ProjectionRebuildCheckpointKey key,
        ProjectionRebuildCheckpoint checkpoint)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(checkpoint);

        return new OrderingProjectionRebuildCheckpoint(key, checkpoint);
    }

    internal static OrderingProjectionRebuildCheckpoint CreateEmpty() => new();
}
