namespace Administration.Persistence.PostgreSqlMigrations;

using Administration.Persistence;
using Microsoft.EntityFrameworkCore.Design;
using Shared.Infrastructure.Persistence;

public sealed class AdministrationPostgreSqlDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AdminDbContext>
{
    public AdminDbContext CreateDbContext(string[] args)
    {
        return new AdminDbContext(
            DesignTimeDbContextOptionsFactory.CreatePostgreSqlOptions<AdminDbContext>(
                args,
                AdminMigrations.PostgreSqlAssembly,
                AdminMigrations.Schema,
                AdminMigrations.HistoryTable));
    }
}
