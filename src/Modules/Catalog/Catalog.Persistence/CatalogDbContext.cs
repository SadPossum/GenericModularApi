namespace Catalog.Persistence;

using Catalog.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Tenancy;
using Shared.Infrastructure.Messaging;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options, ITenantContext tenantContext)
    : DbContext(options)
{
    private readonly bool tenantFilteringEnabled = tenantContext.IsEnabled;
    private readonly string tenantId = tenantContext.TenantId ?? string.Empty;

    public DbSet<CatalogItem> CatalogItems => this.Set<CatalogItem>();
    public DbSet<OutboxMessage> OutboxMessages => this.Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => this.Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(CatalogMigrations.Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);

        modelBuilder.Entity<CatalogItem>()
            .HasQueryFilter("TenantFilter", item => !this.tenantFilteringEnabled || item.TenantId == this.tenantId);
    }
}
