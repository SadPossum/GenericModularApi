namespace Auth.Persistence;

using Auth.Contracts;

public static class AuthMigrations
{
    public const string Schema = AuthModuleMetadata.Schema;
    public const string HistoryTable = "__ef_migrations_history";
    public const string SqlServerAssembly = "Auth.Persistence.SqlServerMigrations";
    public const string PostgreSqlAssembly = "Auth.Persistence.PostgreSqlMigrations";
}
