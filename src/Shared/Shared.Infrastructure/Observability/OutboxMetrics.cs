namespace Shared.Infrastructure.Observability;

using System.Diagnostics;
using System.Diagnostics.Metrics;
using Shared.Application.Observability;

internal sealed class OutboxMetrics
{
    private readonly Counter<long> claimed;
    private readonly Counter<long> published;
    private readonly Counter<long> failed;
    private readonly Histogram<double> publishDuration;

    public OutboxMetrics(IMeterFactory meterFactory)
    {
        Meter meter = meterFactory.Create(ObservabilityMeterNames.Messaging);
        this.claimed = meter.CreateCounter<long>(
            ObservabilityInstrumentNames.OutboxClaimed,
            description: "Number of outbox messages claimed for publishing.");
        this.published = meter.CreateCounter<long>(
            ObservabilityInstrumentNames.OutboxPublished,
            description: "Number of outbox messages published successfully.");
        this.failed = meter.CreateCounter<long>(
            ObservabilityInstrumentNames.OutboxFailed,
            description: "Number of outbox publish attempts that failed.");
        this.publishDuration = meter.CreateHistogram<double>(
            ObservabilityInstrumentNames.OutboxPublishDuration,
            unit: "ms",
            description: "Outbox publish attempt duration in milliseconds.");
    }

    public void RecordClaimed(string moduleName, int count)
    {
        if (count <= 0)
        {
            return;
        }

        this.claimed.Add(
            count,
            new KeyValuePair<string, object?>(ObservabilityTagNames.Module, MetricTagValues.Module(moduleName)));
    }

    public void RecordPublished(string moduleName, string subject, TimeSpan elapsed) =>
        this.RecordPublishAttempt(moduleName, subject, isSuccess: true, elapsed);

    public void RecordFailed(string moduleName, string subject, TimeSpan elapsed) =>
        this.RecordPublishAttempt(moduleName, subject, isSuccess: false, elapsed);

    private void RecordPublishAttempt(string moduleName, string subject, bool isSuccess, TimeSpan elapsed)
    {
        TagList tags = new()
        {
            { ObservabilityTagNames.Module, MetricTagValues.Module(moduleName) },
            { ObservabilityTagNames.Subject, MetricTagValues.Subject(subject) },
            { ObservabilityTagNames.Result, MetricTagValues.Result(isSuccess ? "success" : "failure") },
        };

        if (isSuccess)
        {
            this.published.Add(1, tags);
        }
        else
        {
            this.failed.Add(1, tags);
        }

        this.publishDuration.Record(elapsed.TotalMilliseconds, tags);
    }
}
