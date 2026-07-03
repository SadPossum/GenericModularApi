namespace Shared.Infrastructure.Persistence;

public sealed class PersistenceOptions
{
    public const string SectionName = "Persistence";

    public DatabaseProvider Provider { get; set; } = DatabaseProvider.SqlServer;
}
