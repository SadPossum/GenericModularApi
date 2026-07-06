namespace Shared.Tenancy.Messaging.Infrastructure;

using Shared.Messaging;
using Shared.Tenancy.Messaging;

internal sealed class TenantIntegrationEventScopeResolver : IIntegrationEventScopeResolver
{
    public string? ResolveScopeId(IIntegrationEvent integrationEvent) =>
        integrationEvent is ITenantIntegrationEvent tenantIntegrationEvent
            ? tenantIntegrationEvent.TenantId
            : null;
}
