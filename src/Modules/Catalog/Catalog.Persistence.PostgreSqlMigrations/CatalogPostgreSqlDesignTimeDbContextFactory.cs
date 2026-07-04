namespace Catalog.Persistence.PostgreSqlMigrations;

using Catalog.Persistence;
using Microsoft.EntityFrameworkCore.Design;
using Shared.Persistence.EntityFrameworkCore;

public sealed class CatalogPostgreSqlDesignTimeDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        return new CatalogDbContext(
            DesignTimeDbContextOptionsFactory.CreatePostgreSqlOptions<CatalogDbContext>(
                args,
                CatalogMigrations.PostgreSqlAssembly,
                CatalogMigrations.Schema,
                CatalogMigrations.HistoryTable),
            new DesignTimeTenantContext());
    }
}
