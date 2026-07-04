namespace Shared.Persistence.EntityFrameworkCore;

using Shared.Tenancy;

public sealed class DesignTimeTenantContext : ITenantContext
{
    public bool IsEnabled => false;
    public string? TenantId => "default";
}
