namespace Shared.Infrastructure.Messaging;

using System.Text.Json;
using Shared.Application.Messaging;
using Shared.Domain;

public static class IntegrationEventEnvelopeFactory
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static IntegrationEventEnvelope Create<TEvent>(
        string moduleName,
        TEvent integrationEvent,
        string subjectPrefix = NatsJetStreamOptions.SubjectPrefix)
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
