namespace Shared.Tenancy.Cqrs;

using Shared.Cqrs.Infrastructure;
using Shared.Observability;
using Shared.Tenancy;

internal sealed class TenantCqrsLogScopeContributor(ITenantContext tenantContext) : ICqrsLogScopeContributor
{
    public void Enrich(CqrsLogScopeContext context, IDictionary<string, object?> scopeProperties)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(scopeProperties);

        scopeProperties[ObservabilityLogPropertyNames.TenantId] = tenantContext.TenantId;
    }
}
