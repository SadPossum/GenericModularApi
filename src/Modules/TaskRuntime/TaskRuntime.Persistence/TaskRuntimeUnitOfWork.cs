namespace TaskRuntime.Persistence;

using Shared.Application.UnitOfWork;
using TaskRuntime.Contracts;

internal sealed class TaskRuntimeUnitOfWork(TaskRuntimeDbContext dbContext) : IUnitOfWork
{
    public string ModuleName => TaskRuntimeModuleMetadata.Name;

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.ChangeTracker.HasChanges()
            ? dbContext.SaveChangesAsync(cancellationToken)
            : Task.CompletedTask;
}
