namespace Shared.Domain.Models;

using Shared.Naming;

public abstract class TenantEntity<TId> : Entity<TId>, ITenantScoped
    where TId : notnull
{
    protected TenantEntity() { }

    protected TenantEntity(TId id, string tenantId)
        : base(id)
        => this.TenantId = TenantIds.Normalize(tenantId);

    public string TenantId { get; private set; } = string.Empty;
}
