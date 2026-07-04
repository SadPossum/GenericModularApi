namespace Ordering.Persistence;

using Shared.Naming;
using Shared.ProjectionRebuild;
using Shared.Tasks;

public sealed class OrderingProjectionRebuildCheckpoint
{
    public const int ProjectionNameMaxLength = 128;
    public const int CursorMaxLength = 512;

    private OrderingProjectionRebuildCheckpoint() { }

    private OrderingProjectionRebuildCheckpoint(
        ProjectionRebuildCheckpointKey key,
        ProjectionRebuildCheckpoint checkpoint)
    {
        this.RunId = key.RunId;
        this.TenantId = RequireTenantId(key.TenantId);
        this.ProjectionName = TaskNames.NormalizeTaskName(key.ProjectionName);
        this.Apply(checkpoint);
    }

    public Guid RunId { get; private set; }
    public string TenantId { get; private set; } = string.Empty;
    public string ProjectionName { get; private set; } = string.Empty;
    public string? Cursor { get; private set; }
    public long ProcessedCount { get; private set; }
    public long WrittenCount { get; private set; }
    public long SkippedCount { get; private set; }
    public long FailedCount { get; private set; }
    public int ProjectionVersion { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public static OrderingProjectionRebuildCheckpoint Create(
        ProjectionRebuildCheckpointKey key,
        ProjectionRebuildCheckpoint checkpoint)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(checkpoint);

        return new OrderingProjectionRebuildCheckpoint(key, checkpoint);
    }

    public void Update(ProjectionRebuildCheckpoint checkpoint)
    {
        ArgumentNullException.ThrowIfNull(checkpoint);
        this.Apply(checkpoint);
    }

    public ProjectionRebuildCheckpoint ToCheckpoint() =>
        new(
            this.Cursor,
            this.ProcessedCount,
            this.WrittenCount,
            this.SkippedCount,
            this.FailedCount,
            this.ProjectionVersion,
            this.UpdatedAtUtc,
            this.CompletedAtUtc);

    private void Apply(ProjectionRebuildCheckpoint checkpoint)
    {
        this.Cursor = NormalizeCursor(checkpoint.Cursor);
        this.ProcessedCount = checkpoint.ProcessedCount;
        this.WrittenCount = checkpoint.WrittenCount;
        this.SkippedCount = checkpoint.SkippedCount;
        this.FailedCount = checkpoint.FailedCount;
        this.ProjectionVersion = checkpoint.ProjectionVersion;
        this.UpdatedAtUtc = checkpoint.UpdatedAtUtc;
        this.CompletedAtUtc = checkpoint.CompletedAtUtc;
    }

    private static string RequireTenantId(string? tenantId) =>
        string.IsNullOrWhiteSpace(tenantId)
            ? throw new InvalidOperationException("Ordering projection rebuild checkpoints require a tenant id.")
            : TenantIds.Normalize(tenantId);

    private static string? NormalizeCursor(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
        {
            return null;
        }

        string normalized = cursor.Trim();
        if (normalized.Length > CursorMaxLength || normalized.Any(char.IsControl))
        {
            throw new ArgumentException(
                $"Projection rebuild cursor must be {CursorMaxLength} characters or fewer and cannot contain control characters.",
                nameof(cursor));
        }

        return normalized;
    }
}
