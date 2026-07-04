namespace Shared.Messaging.Nats;

using Shared.Messaging;

public sealed class NatsJetStreamOptions
{
    public const string SectionName = "NatsJetStream";

    public bool Enabled { get; set; }
    public string StreamName { get; set; } = "GMA_EVENTS";

    public static string SubjectWildcard => $"{IntegrationEventNaming.DefaultSubjectPrefix}.>";
}
