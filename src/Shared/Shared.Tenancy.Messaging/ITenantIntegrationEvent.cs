namespace Shared.Tenancy.Messaging;

using Shared.Messaging;

public interface ITenantIntegrationEvent : IIntegrationEvent
{
    string TenantId { get; }
}
