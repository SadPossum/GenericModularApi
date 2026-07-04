namespace Auth.Persistence.PostgreSqlMigrations;

using Auth.Persistence;
using Microsoft.EntityFrameworkCore.Design;
using Shared.Persistence.EntityFrameworkCore;

public sealed class AuthPostgreSqlDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        return new AuthDbContext(
            DesignTimeDbContextOptionsFactory.CreatePostgreSqlOptions<AuthDbContext>(
                args,
                AuthMigrations.PostgreSqlAssembly,
                AuthMigrations.Schema,
                AuthMigrations.HistoryTable),
            new DesignTimeTenantContext());
    }
}
