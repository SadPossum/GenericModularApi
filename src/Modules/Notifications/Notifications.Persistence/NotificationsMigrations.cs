namespace Notifications.Persistence;

using Notifications.Contracts;

public static class NotificationsMigrations
{
    public const string Schema = NotificationsModuleMetadata.Schema;
    public const string HistoryTable = "__ef_migrations_history";
    public const string SqlServerAssembly = "Notifications.Persistence.SqlServerMigrations";
    public const string PostgreSqlAssembly = "Notifications.Persistence.PostgreSqlMigrations";
}
