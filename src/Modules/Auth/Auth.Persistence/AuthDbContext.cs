namespace Auth.Persistence;

using Auth.Domain.Aggregates;
using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Tenancy;
using Shared.Infrastructure.Messaging;

public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options, ITenantContext tenantContext)
    : DbContext(options)
{
    private readonly bool tenantFilteringEnabled = tenantContext.IsEnabled;
    private readonly string tenantId = tenantContext.TenantId ?? string.Empty;

    public DbSet<Member> Members => this.Set<Member>();
    public DbSet<MemberUsername> MemberUsernames => this.Set<MemberUsername>();
    public DbSet<MemberSession> MemberSessions => this.Set<MemberSession>();
    public DbSet<OutboxMessage> OutboxMessages => this.Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => this.Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(AuthMigrations.Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);

        modelBuilder.Entity<Member>()
            .HasQueryFilter("TenantFilter", member => !this.tenantFilteringEnabled || member.TenantId == this.tenantId);

        modelBuilder.Entity<MemberUsername>()
            .HasQueryFilter("TenantFilter", username => !this.tenantFilteringEnabled || username.TenantId == this.tenantId);

        modelBuilder.Entity<MemberSession>()
            .HasQueryFilter("TenantFilter", session => !this.tenantFilteringEnabled || session.TenantId == this.tenantId);
    }
}
