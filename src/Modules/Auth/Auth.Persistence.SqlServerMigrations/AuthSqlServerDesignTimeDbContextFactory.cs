namespace Auth.Persistence.SqlServerMigrations;

using Auth.Persistence;
using Microsoft.EntityFrameworkCore.Design;
using Shared.Persistence.EntityFrameworkCore;

public sealed class AuthSqlServerDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        return new AuthDbContext(
            DesignTimeDbContextOptionsFactory.CreateSqlServerOptions<AuthDbContext>(
                args,
                AuthMigrations.SqlServerAssembly,
                AuthMigrations.Schema,
                AuthMigrations.HistoryTable),
            new DesignTimeTenantContext());
    }
}
