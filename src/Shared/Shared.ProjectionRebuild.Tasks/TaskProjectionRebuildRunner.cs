namespace Shared.ProjectionRebuild.Tasks;

using Shared.ProjectionRebuild;
using Shared.Tasks;

public sealed class TaskProjectionRebuildRunner<TSnapshot>(
    ProjectionRebuildRunner<TSnapshot> runner,
    ITaskRuntimeReporter reporter,
    ITaskControlLoop controlLoop)
{
    public Task<ProjectionRebuildSummary> RunAsync(
        string moduleName,
        ProjectionRebuildRequest request,
        IProjectionRebuildSource<TSnapshot> source,
        IProjectionRebuildWriter<TSnapshot> writer,
        TaskExecutionContext context,
        bool tenantScoped,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        ProjectionRebuildExecutionContext rebuildContext = new(
            context.RunId,
            tenantScoped ? context.TenantId : null);
        TaskProjectionRebuildRunObserver observer = new(context, reporter, controlLoop);

        return runner.RunAsync(
            moduleName,
            request,
            source,
            writer,
            rebuildContext,
            tenantScoped,
            observer,
            cancellationToken);
    }
}
