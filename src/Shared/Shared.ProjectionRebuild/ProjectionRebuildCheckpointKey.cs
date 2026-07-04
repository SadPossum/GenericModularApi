namespace Shared.ProjectionRebuild;

using Shared.Naming;
using Shared.Tasks;

public sealed record ProjectionRebuildCheckpointKey
{
    public ProjectionRebuildCheckpointKey(
        string moduleName,
        Guid runId,
        string projectionName,
        string? tenantId)
    {
        this.ModuleName = TaskNames.NormalizeModuleName(moduleName, nameof(moduleName));
        this.RunId = runId == Guid.Empty
            ? throw new ArgumentException("Projection rebuild run id must not be empty.", nameof(runId))
            : runId;
        this.ProjectionName = TaskNames.NormalizeTaskName(projectionName, nameof(projectionName));
        this.TenantId = string.IsNullOrWhiteSpace(tenantId)
            ? null
            : TenantIds.Normalize(tenantId);
    }

    public string ModuleName { get; }
    public Guid RunId { get; }
    public string ProjectionName { get; }
    public string? TenantId { get; }
}
