namespace Administration.Persistence;

using Administration.Contracts;

public static class AdminMigrations
{
    public const string Schema = AdministrationModuleMetadata.Schema;
    public const string HistoryTable = "__ef_migrations_history";
    public const string SqlServerAssembly = "Administration.Persistence.SqlServerMigrations";
    public const string PostgreSqlAssembly = "Administration.Persistence.PostgreSqlMigrations";
}
