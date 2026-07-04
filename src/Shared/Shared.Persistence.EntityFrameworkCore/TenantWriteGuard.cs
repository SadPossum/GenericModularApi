namespace Shared.Persistence.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared.Domain;
using Shared.Naming;
using Shared.Tenancy;

public static class TenantWriteGuard
{
    public static void ValidateTenantScopedWrites(this ChangeTracker changeTracker, ITenantContext tenantContext)
    {
        ArgumentNullException.ThrowIfNull(changeTracker);
        ArgumentNullException.ThrowIfNull(tenantContext);

        string? activeTenantId = null;
        if (tenantContext.IsEnabled)
        {
            if (!TenantIds.TryNormalize(tenantContext.TenantId, out activeTenantId))
            {
                throw new TenantWriteGuardException("Tenant-aware writes require a valid active tenant id.");
            }
        }

        foreach (EntityEntry<ITenantScoped> entry in changeTracker
                     .Entries<ITenantScoped>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            string entityName = entry.Metadata.ClrType.FullName ?? entry.Metadata.ClrType.Name;
            string tenantId = entry.Entity.TenantId;

            if (!TenantIds.TryNormalize(tenantId, out string? normalizedTenantId) ||
                !string.Equals(normalizedTenantId, tenantId, StringComparison.Ordinal))
            {
                throw new TenantWriteGuardException(
                    $"{entityName} has an invalid or unnormalized tenant id.");
            }

            if (tenantContext.IsEnabled &&
                !string.Equals(normalizedTenantId, activeTenantId, StringComparison.Ordinal))
            {
                throw new TenantWriteGuardException(
                    $"{entityName} belongs to tenant '{normalizedTenantId}', but the active tenant is '{activeTenantId}'.");
            }
        }
    }
}
