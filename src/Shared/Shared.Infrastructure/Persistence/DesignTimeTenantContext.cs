namespace Shared.Infrastructure.Persistence;

using Shared.Application.Tenancy;

public sealed class DesignTimeTenantContext : ITenantContext
{
    public bool IsEnabled => false;
    public string? TenantId => "default";
}
