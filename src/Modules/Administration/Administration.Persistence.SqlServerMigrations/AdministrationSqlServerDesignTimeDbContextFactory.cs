namespace Administration.Persistence.SqlServerMigrations;

using Administration.Persistence;
using Microsoft.EntityFrameworkCore.Design;
using Shared.Infrastructure.Persistence;

public sealed class AdministrationSqlServerDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AdminDbContext>
{
    public AdminDbContext CreateDbContext(string[] args)
    {
        return new AdminDbContext(
            DesignTimeDbContextOptionsFactory.CreateSqlServerOptions<AdminDbContext>(
                args,
                AdminMigrations.SqlServerAssembly,
                AdminMigrations.Schema,
                AdminMigrations.HistoryTable));
    }
}
