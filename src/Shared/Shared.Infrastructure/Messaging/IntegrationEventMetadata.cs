namespace Shared.Infrastructure.Messaging;

using Shared.Application.Messaging;
using Shared.Domain;

internal static class IntegrationEventMetadata
{
    public const string EventIdRequiredReason = "event-id-required";
    public const string EventNameRequiredReason = "event-name-required";
    public const string EventNameInvalidReason = "event-name-invalid";
    public const string EventVersionInvalidReason = "event-version-invalid";
    public const string TenantIdInvalidReason = "tenant-id-invalid";

    public static void ValidateForPublishing(IIntegrationEvent integrationEvent)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        if (integrationEvent.EventId == Guid.Empty)
        {
            throw new ArgumentException("An integration event id is required.", nameof(integrationEvent));
        }

        _ = IntegrationEventNaming.NormalizeEventName(integrationEvent.EventName, nameof(IIntegrationEvent.EventName));
        ArgumentOutOfRangeException.ThrowIfLessThan(integrationEvent.Version, 1, nameof(IIntegrationEvent.Version));
        _ = TenantIds.Normalize(integrationEvent.TenantId);
    }

    public static bool TryGetInvalidReason(IIntegrationEvent integrationEvent, out string reason)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        if (integrationEvent.EventId == Guid.Empty)
        {
            reason = EventIdRequiredReason;
            return true;
        }

        if (string.IsNullOrWhiteSpace(integrationEvent.EventName))
        {
            reason = EventNameRequiredReason;
            return true;
        }

        if (!IntegrationEventNaming.TryNormalizeEventName(integrationEvent.EventName, out _))
        {
            reason = EventNameInvalidReason;
            return true;
        }

        if (integrationEvent.Version < 1)
        {
            reason = EventVersionInvalidReason;
            return true;
        }

        if (!TenantIds.TryNormalize(integrationEvent.TenantId, out _))
        {
            reason = TenantIdInvalidReason;
            return true;
        }

        reason = string.Empty;
        return false;
    }
}
