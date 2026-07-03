namespace Shared.Infrastructure.Observability;

using System.Diagnostics;
using System.Diagnostics.Metrics;
using Shared.Application.Observability;

internal sealed class QueryMetrics
{
    private readonly Counter<long> executed;
    private readonly Histogram<double> duration;

    public QueryMetrics(IMeterFactory meterFactory)
    {
        Meter meter = meterFactory.Create(ObservabilityMeterNames.Application);
        this.executed = meter.CreateCounter<long>(
            ObservabilityInstrumentNames.QueriesExecuted,
            description: "Number of queries completed by the application pipeline.");
        this.duration = meter.CreateHistogram<double>(
            ObservabilityInstrumentNames.QueriesDuration,
            unit: "ms",
            description: "Query execution duration in milliseconds.");
    }

    public void Record(string moduleName, string queryName, bool isSuccess, string? errorCode, TimeSpan elapsed)
    {
        string? normalizedErrorCode = MetricTagValues.ErrorCode(errorCode);
        TagList tags = new()
        {
            { ObservabilityTagNames.Module, MetricTagValues.Module(moduleName) },
            { ObservabilityTagNames.Operation, MetricTagValues.Operation(queryName) },
            { ObservabilityTagNames.Result, MetricTagValues.Result(isSuccess ? "success" : "failure") },
        };

        if (normalizedErrorCode is not null)
        {
            tags.Add(ObservabilityTagNames.ErrorCode, normalizedErrorCode);
        }

        this.executed.Add(1, tags);
        this.duration.Record(elapsed.TotalMilliseconds, tags);
    }
}
