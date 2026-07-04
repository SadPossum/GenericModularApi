namespace Shared.Observability;

public static class ObservabilityInstrumentNames
{
    public const string CommandsExecuted = "gma.commands.executed";
    public const string CommandsDuration = "gma.commands.duration";
    public const string QueriesExecuted = "gma.queries.executed";
    public const string QueriesDuration = "gma.queries.duration";

    public const string OutboxClaimed = "gma.outbox.claimed";
    public const string OutboxPublished = "gma.outbox.published";
    public const string OutboxFailed = "gma.outbox.failed";
    public const string OutboxPublishDuration = "gma.outbox.publish.duration";
    public const string InboxMessages = "gma.inbox.messages";
    public const string InboxProcessDuration = "gma.inbox.process.duration";

    public const string CacheRequests = "gma.cache.requests";
    public const string CacheDuration = "gma.cache.duration";
    public const string CacheBackendFailures = "gma.cache.backend.failures";
    public const string CacheInvalidationFailures = "gma.cache.invalidation.failures";

    public const string TaskClaimed = "gma.tasks.claimed";
    public const string TaskCompleted = "gma.tasks.completed";
    public const string TaskDuration = "gma.tasks.duration";
    public const string TaskTimedOut = "gma.tasks.timed_out";
    public const string TaskQueueDepth = "gma.tasks.queue.depth";
    public const string TaskActiveRuns = "gma.tasks.active.runs";
}
