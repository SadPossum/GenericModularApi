namespace Shared.Infrastructure.Messaging;

public sealed class NatsJetStreamOptions
{
    public const string SectionName = "NatsJetStream";
    public const string SubjectPrefix = "gma";

    public bool Enabled { get; set; }
    public string StreamName { get; set; } = "GMA_EVENTS";

    public static string SubjectWildcard => $"{SubjectPrefix}.>";
}
