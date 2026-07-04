namespace Shared.Messaging.Infrastructure;

using System.Diagnostics;
using System.Diagnostics.Metrics;
using Shared.Messaging;
using Shared.Observability;
using Shared.Observability.Infrastructure;

public sealed class InboxMetrics
{
    private readonly Counter<long> messages;
    private readonly Histogram<double> processDuration;

    public InboxMetrics(IMeterFactory meterFactory)
    {
        Meter meter = meterFactory.Create(ObservabilityMeterNames.Messaging);
        this.messages = meter.CreateCounter<long>(
            ObservabilityInstrumentNames.InboxMessages,
            description: "Number of inbox message processing outcomes.");
        this.processDuration = meter.CreateHistogram<double>(
            ObservabilityInstrumentNames.InboxProcessDuration,
            unit: "ms",
            description: "Inbox message processing duration in milliseconds.");
    }

    public void RecordProcessed(
        string moduleName,
        string handlerName,
        string subject,
        InboxProcessStatus status,
        TimeSpan elapsed)
    {
        TagList tags = new()
        {
            { ObservabilityTagNames.Module, MetricTagValues.Module(moduleName) },
            { ObservabilityTagNames.Operation, MetricTagValues.Operation(handlerName) },
            { ObservabilityTagNames.Subject, IntegrationEventNaming.NormalizeSubject(subject) },
            { ObservabilityTagNames.Result, MetricTagValues.Result(NormalizeStatus(status)) }
        };

        this.messages.Add(1, tags);
        this.processDuration.Record(elapsed.TotalMilliseconds, tags);
    }

    private static string NormalizeStatus(InboxProcessStatus status) =>
        status switch
        {
            InboxProcessStatus.Processed => "processed",
            InboxProcessStatus.Duplicate => "duplicate",
            InboxProcessStatus.Failed => "failed",
            _ => "unknown"
        };
}
