namespace Ordering.Contracts;

using Shared.Tasks;
using Shared.Tenancy;

[TaskName(TaskName)]
[TaskPayloadVersion(PayloadVersion)]
[TaskDescription("Rebuild Ordering's local catalog item projection from Catalog exports.")]
[TaskKind(ModuleTaskKind.OneShot)]
[TaskWorkerGroup(OrderingModuleMetadata.ProjectionWorkerGroup)]
[SupportsTaskControl]
[TenantScoped]
public sealed record RebuildCatalogItemProjectionPayload(
    int ProjectionVersion = OrderingModuleMetadata.CatalogItemProjectionVersion,
    int BatchSize = RebuildCatalogItemProjectionPayload.DefaultBatchSize,
    bool DryRun = false,
    string? Cursor = null) : ITaskPayload
{
    public const string TaskName = "rebuild-catalog-item-projections";
    public const int PayloadVersion = 1;
    public const int DefaultBatchSize = 500;
}
