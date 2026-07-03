namespace TaskRuntime.Persistence.SqlServerMigrations;

using Microsoft.EntityFrameworkCore.Design;
using Shared.Infrastructure.Persistence;
using TaskRuntime.Persistence;

public sealed class TaskRuntimeSqlServerDesignTimeDbContextFactory : IDesignTimeDbContextFactory<TaskRuntimeDbContext>
{
    public TaskRuntimeDbContext CreateDbContext(string[] args)
    {
        return new TaskRuntimeDbContext(
            DesignTimeDbContextOptionsFactory.CreateSqlServerOptions<TaskRuntimeDbContext>(
                args,
                TaskRuntimeMigrations.SqlServerAssembly,
                TaskRuntimeMigrations.Schema,
                TaskRuntimeMigrations.HistoryTable));
    }
}
