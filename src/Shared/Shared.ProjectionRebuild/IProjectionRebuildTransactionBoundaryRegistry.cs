namespace Shared.ProjectionRebuild;

public interface IProjectionRebuildTransactionBoundaryRegistry
{
    IProjectionRebuildTransactionBoundary? GetOptional(string moduleName);
}
