namespace Ordering.Application.Tasks;

using Ordering.Contracts;
using Shared.ProjectionRebuild;
using Shared.Tasks;

public sealed record RebuildCatalogItemProjectionPayload(
    int ProjectionVersion = OrderingModuleMetadata.CatalogItemProjectionVersion,
    int BatchSize = ProjectionRebuildRequest.DefaultBatchSize,
    bool DryRun = false,
    string? Cursor = null) : ITaskPayload;
