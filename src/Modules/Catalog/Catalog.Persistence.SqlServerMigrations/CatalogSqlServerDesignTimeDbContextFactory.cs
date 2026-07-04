namespace Catalog.Persistence.SqlServerMigrations;

using Catalog.Persistence;
using Microsoft.EntityFrameworkCore.Design;
using Shared.Persistence.EntityFrameworkCore;

public sealed class CatalogSqlServerDesignTimeDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        return new CatalogDbContext(
            DesignTimeDbContextOptionsFactory.CreateSqlServerOptions<CatalogDbContext>(
                args,
                CatalogMigrations.SqlServerAssembly,
                CatalogMigrations.Schema,
                CatalogMigrations.HistoryTable),
            new DesignTimeTenantContext());
    }
}
