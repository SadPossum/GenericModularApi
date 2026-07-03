namespace TaskRuntime.Persistence;

using TaskRuntime.Contracts;

public static class TaskRuntimeMigrations
{
    public const string Schema = TaskRuntimeModuleMetadata.Schema;
    public const string HistoryTable = "__ef_migrations_history";
    public const string SqlServerAssembly = "TaskRuntime.Persistence.SqlServerMigrations";
    public const string PostgreSqlAssembly = "TaskRuntime.Persistence.PostgreSqlMigrations";
}
