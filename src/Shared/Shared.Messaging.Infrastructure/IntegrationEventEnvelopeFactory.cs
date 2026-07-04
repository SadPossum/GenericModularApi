namespace Shared.Messaging.Infrastructure;

using Shared.Naming;
using System.Text.Json;
using Shared.Messaging;

public static class IntegrationEventEnvelopeFactory
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static IntegrationEventEnvelope Create<TEvent>(
        string moduleName,
        TEvent integrationEvent,
        string subjectPrefix = IntegrationEventNaming.DefaultSubjectPrefix)
        where TEvent : IIntegrationEvent
    {
        IntegrationEventMetadata.ValidateForPublishing(integrationEvent);

        string tenantId = TenantIds.Normalize(integrationEvent.TenantId);
        string subject = IntegrationEventNaming.CreateSubject(
            subjectPrefix,
            moduleName,
            integrationEvent.EventName,
            integrationEvent.Version);
        string payload = JsonSerializer.Serialize(integrationEvent, JsonOptions);

        return new(
            integrationEvent.EventId,
            subject,
            typeof(TEvent).FullName ?? typeof(TEvent).Name,
            integrationEvent.Version,
            tenantId,
            integrationEvent.OccurredAtUtc,
            payload);
    }
}
