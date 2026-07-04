namespace TaskRuntime.Persistence.PostgreSqlMigrations;

using Microsoft.EntityFrameworkCore.Design;
using Shared.Persistence.EntityFrameworkCore;
using TaskRuntime.Persistence;

public sealed class TaskRuntimePostgreSqlDesignTimeDbContextFactory : IDesignTimeDbContextFactory<TaskRuntimeDbContext>
{
    public TaskRuntimeDbContext CreateDbContext(string[] args)
    {
        return new TaskRuntimeDbContext(
            DesignTimeDbContextOptionsFactory.CreatePostgreSqlOptions<TaskRuntimeDbContext>(
                args,
                TaskRuntimeMigrations.PostgreSqlAssembly,
                TaskRuntimeMigrations.Schema,
                TaskRuntimeMigrations.HistoryTable));
    }
}
