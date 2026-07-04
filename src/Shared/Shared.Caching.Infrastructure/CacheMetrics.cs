namespace Shared.Caching.Infrastructure;

using System.Diagnostics;
using System.Diagnostics.Metrics;
using Shared.Observability;
using Shared.Observability.Infrastructure;

internal sealed class CacheMetrics(IMeterFactory meterFactory)
{
    private readonly Counter<long> requests = meterFactory
        .Create(ObservabilityMeterNames.Caching)
        .CreateCounter<long>(ObservabilityInstrumentNames.CacheRequests);
    private readonly Histogram<double> duration = meterFactory
        .Create(ObservabilityMeterNames.Caching)
        .CreateHistogram<double>(ObservabilityInstrumentNames.CacheDuration, "ms");
    private readonly Counter<long> backendFailures = meterFactory
        .Create(ObservabilityMeterNames.Caching)
        .CreateCounter<long>(ObservabilityInstrumentNames.CacheBackendFailures);
    private readonly Counter<long> invalidationFailures = meterFactory
        .Create(ObservabilityMeterNames.Caching)
        .CreateCounter<long>(ObservabilityInstrumentNames.CacheInvalidationFailures);

    public void RecordRequest(string module, string provider, string result, TimeSpan elapsed)
    {
        TagList tags = CreateTags(module, "read", provider, result);
        this.requests.Add(1, tags);
        this.duration.Record(elapsed.TotalMilliseconds, tags);
    }

    public void RecordBackendFailure(string module, string operation, string provider) =>
        this.backendFailures.Add(1, CreateTags(module, operation, provider, "failure"));

    public void RecordInvalidationFailure(string module, string operation, string provider) =>
        this.invalidationFailures.Add(1, CreateTags(module, operation, provider, "failure"));

    private static TagList CreateTags(string module, string operation, string provider, string result) => new()
    {
        { ObservabilityTagNames.Module, MetricTagValues.Module(module) },
        { ObservabilityTagNames.Operation, MetricTagValues.Operation(operation) },
        { ObservabilityTagNames.Provider, MetricTagValues.Provider(provider) },
        { ObservabilityTagNames.Result, MetricTagValues.Result(result) }
    };
}
