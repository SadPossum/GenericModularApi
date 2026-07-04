namespace Shared.Tenancy;

public interface ITenantContextAccessor : ITenantContext
{
    void SetTenant(string tenantId);
    void ClearTenant();
}
