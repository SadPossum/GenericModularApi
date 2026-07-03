namespace Shared.Application.Tenancy;

public interface ITenantContextAccessor : ITenantContext
{
    void SetTenant(string tenantId);
    void ClearTenant();
}
