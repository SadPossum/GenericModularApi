namespace Shared.Tenancy.Messaging;

using Shared.Messaging;
using Shared.Naming;

public abstract record TenantIntegrationEvent : IntegrationEvent, ITenantIntegrationEvent
{
    protected TenantIntegrationEvent(
        Guid eventId,
        string tenantId,
        DateTimeOffset occurredAtUtc,
        string eventName,
        int version)
        : base(eventId, occurredAtUtc, eventName, version)
        => this.TenantId = TenantIds.Normalize(tenantId);

    public string TenantId { get; }
}
