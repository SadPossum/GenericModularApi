namespace Shared.Tests;

using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Shared.Observability;
using Shared.ProjectionRebuild;
using Shared.Runtime;
using Shared.Runtime.Time;
using Shared.Tasks;
using Xunit;

[Trait("Category", "Unit")]
[Collection(MetricsTestGroupDefinition.Name)]
public sealed class ProjectionRebuildTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 4, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid RunId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    [Fact]
    public void Contracts_validate_counts_cursors_tenant_and_batch_size()
    {
        ProjectionRebuildRequest request = new(
            " Catalog-Projection ",
            projectionVersion: 2,
            batchSize: 5,
            dryRun: true,
            cursor: " cursor-1 ");
        ProjectionRebuildCheckpointKey key = new(
            " Ordering ",
            RunId,
            " Catalog-Projection ",
            " tenant-a ");
        ProjectionRebuildCheckpoint checkpoint = ProjectionRebuildCheckpoint
            .Start(2, Now, " cursor-1 ")
            .Advance(" cursor-2 ", 3, new ProjectionWriteResult(2, skippedCount: 1), Now.AddSeconds(1))
            .Complete(Now.AddSeconds(2));

        Assert.Equal("catalog-projection", request.ProjectionName);
        Assert.Equal(2, request.ProjectionVersion);
        Assert.Equal(5, request.BatchSize);
        Assert.True(request.DryRun);
        Assert.Equal("cursor-1", request.Cursor);
        Assert.Equal("ordering", key.ModuleName);
        Assert.Equal("catalog-projection", key.ProjectionName);
        Assert.Equal("tenant-a", key.TenantId);
        Assert.True(checkpoint.IsCompleted);
        Assert.Equal("cursor-2", checkpoint.Cursor);
        Assert.Equal(3, checkpoint.ProcessedCount);
        Assert.Equal(2, checkpoint.WrittenCount);
        Assert.Equal(1, checkpoint.SkippedCount);
        Assert.Equal(Now.AddSeconds(2), checkpoint.CompletedAtUtc);

        Assert.Throws<ArgumentException>(() => new ProjectionReadBatch<string>(["a"], null, hasMore: true));
        Assert.Throws<ArgumentException>(() => new ProjectionReadBatch<string>(["a"], "\u0001", hasMore: false));
        Assert.Throws<ArgumentException>(() => new ProjectionRebuildRequest("bad name", 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ProjectionRebuildRequest("catalog-projection", 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ProjectionRebuildRequest("catalog-projection", 1, batchSize: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ProjectionRebuildRequest(
            "catalog-projection",
            1,
            batchSize: ProjectionRebuildRequest.MaxBatchSize + 1));
        Assert.Throws<ArgumentException>(() => new ProjectionRebuildCheckpointKey(
            "ordering",
            Guid.Empty,
            "catalog-projection",
            "tenant-a"));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ProjectionWriteResult(-1));
        Assert.Throws<ArgumentException>(() => ProjectionRebuildCheckpoint.Start(1, default));
    }

    [Fact]
    public void Registry_resolves_stores_by_normalized_module_and_rejects_missing_or_duplicate_stores()
    {
        RecordingCheckpointStore store = new(" Ordering ");
        using ServiceProvider provider = BuildProvider([store]);
        using IServiceScope scope = provider.CreateScope();
        IProjectionRebuildCheckpointStoreRegistry registry =
            scope.ServiceProvider.GetRequiredService<IProjectionRebuildCheckpointStoreRegistry>();

        Assert.Same(store, registry.GetRequired("ordering"));
        Assert.Throws<InvalidOperationException>(() => registry.GetRequired("catalog"));

        RecordingCheckpointStore duplicate = new("ordering");
        using ServiceProvider duplicateProvider = BuildProvider([store, duplicate]);
        using IServiceScope duplicateScope = duplicateProvider.CreateScope();

        Assert.Throws<InvalidOperationException>(() =>
            duplicateScope.ServiceProvider.GetRequiredService<IProjectionRebuildCheckpointStoreRegistry>());
    }

    [Fact]
    public async Task Runner_processes_batches_persists_checkpoint_reports_progress_and_records_metrics()
    {
        List<MetricMeasurement> measurements = [];
        using MeterListener listener = CreateListener(measurements);
        RecordingCheckpointStore store = new("ordering");
        RecordingReporter reporter = new();
        using ServiceProvider provider = BuildProvider([store], reporter);
        using IServiceScope scope = provider.CreateScope();
        ProjectionRebuildRunner<string> runner =
            scope.ServiceProvider.GetRequiredService<ProjectionRebuildRunner<string>>();
        RecordingSource source = new(
            new ProjectionReadBatch<string>(["a", "b"], "b", hasMore: true),
            new ProjectionReadBatch<string>(["c"], "c", hasMore: false));
        RecordingWriter writer = new(snapshotCount => new ProjectionWriteResult(snapshotCount));
        TaskExecutionContext context = CreateContext();
        ProjectionRebuildRequest request = new("catalog-item-projections", 1, batchSize: 2);

        ProjectionRebuildSummary summary = await runner.RunAsync(
            "ordering",
            request,
            source,
            writer,
            context,
            tenantScoped: true,
            CancellationToken.None);

        Assert.Equal("ordering", summary.ModuleName);
        Assert.Equal("catalog-item-projections", summary.ProjectionName);
        Assert.Equal("tenant-a", summary.TenantId);
        Assert.True(summary.Checkpoint.IsCompleted);
        Assert.Equal("c", summary.Checkpoint.Cursor);
        Assert.Equal(3, summary.Checkpoint.ProcessedCount);
        Assert.Equal(3, summary.Checkpoint.WrittenCount);
        Assert.Equal([null, "b"], source.Cursors);
        Assert.Equal([["a", "b"], ["c"]], writer.WrittenBatches);
        Assert.Equal(2, store.SaveCount);
        Assert.Equal(summary.Checkpoint, Assert.Single(store.Checkpoints.Values));
        Assert.Equal([99, 100], reporter.Progress.Select(progress => progress.PercentComplete));
        Assert.Contains(reporter.Progress, progress =>
            progress.Message is not null &&
            progress.Message.Contains("processed=3", StringComparison.Ordinal) &&
            progress.Message.Contains("cursor=c", StringComparison.Ordinal));
        Assert.Contains(measurements, measurement =>
            measurement.InstrumentName == "gma.projections.rebuild.rows" &&
            measurement.Value == 2 &&
            (string?)measurement.Tags[ObservabilityTagNames.Module] == "ordering" &&
            (string?)measurement.Tags[ObservabilityTagNames.Operation] == "catalog-item-projections" &&
            (string?)measurement.Tags[ObservabilityTagNames.Result] == "success" &&
            !measurement.Tags.Keys.Any(key => key.Contains("tenant", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public async Task Runner_resumes_from_existing_checkpoint_when_payload_cursor_is_not_supplied()
    {
        ProjectionRebuildCheckpointKey key = new(
            "ordering",
            RunId,
            "catalog-item-projections",
            "tenant-a");
        RecordingCheckpointStore store = new("ordering");
        store.Seed(key, new ProjectionRebuildCheckpoint(
            "b",
            processedCount: 2,
            writtenCount: 2,
            skippedCount: 0,
            failedCount: 0,
            projectionVersion: 1,
            updatedAtUtc: Now));
        using ServiceProvider provider = BuildProvider([store]);
        using IServiceScope scope = provider.CreateScope();
        ProjectionRebuildRunner<string> runner =
            scope.ServiceProvider.GetRequiredService<ProjectionRebuildRunner<string>>();
        RecordingSource source = new(new ProjectionReadBatch<string>(["c"], "c", hasMore: false));
        RecordingWriter writer = new(snapshotCount => new ProjectionWriteResult(snapshotCount));

        ProjectionRebuildSummary summary = await runner.RunAsync(
            "ordering",
            new ProjectionRebuildRequest("catalog-item-projections", 1),
            source,
            writer,
            CreateContext(),
            tenantScoped: true,
            CancellationToken.None);

        Assert.Equal(["b"], source.Cursors);
        Assert.Equal(3, summary.Checkpoint.ProcessedCount);
        Assert.Equal(3, summary.Checkpoint.WrittenCount);
        Assert.True(summary.Checkpoint.IsCompleted);
    }

    [Fact]
    public async Task Runner_payload_cursor_override_starts_a_new_rebuild_even_when_checkpoint_exists()
    {
        ProjectionRebuildCheckpointKey key = new(
            "ordering",
            RunId,
            "catalog-item-projections",
            "tenant-a");
        RecordingCheckpointStore store = new("ordering");
        store.Seed(key, new ProjectionRebuildCheckpoint(
            "b",
            processedCount: 2,
            writtenCount: 2,
            skippedCount: 0,
            failedCount: 0,
            projectionVersion: 1,
            updatedAtUtc: Now));
        using ServiceProvider provider = BuildProvider([store]);
        using IServiceScope scope = provider.CreateScope();
        ProjectionRebuildRunner<string> runner =
            scope.ServiceProvider.GetRequiredService<ProjectionRebuildRunner<string>>();
        RecordingSource source = new(new ProjectionReadBatch<string>(["z"], "z", hasMore: false));
        RecordingWriter writer = new(snapshotCount => new ProjectionWriteResult(snapshotCount));

        ProjectionRebuildSummary summary = await runner.RunAsync(
            "ordering",
            new ProjectionRebuildRequest("catalog-item-projections", 1, cursor: " y "),
            source,
            writer,
            CreateContext(),
            tenantScoped: true,
            CancellationToken.None);

        Assert.Equal(["y"], source.Cursors);
        Assert.Equal(1, summary.Checkpoint.ProcessedCount);
        Assert.Equal(1, summary.Checkpoint.WrittenCount);
        Assert.True(summary.Checkpoint.IsCompleted);
    }

    [Fact]
    public async Task Runner_does_not_advance_checkpoint_when_writer_fails_a_batch()
    {
        RecordingCheckpointStore store = new("ordering");
        using ServiceProvider provider = BuildProvider([store]);
        using IServiceScope scope = provider.CreateScope();
        ProjectionRebuildRunner<string> runner =
            scope.ServiceProvider.GetRequiredService<ProjectionRebuildRunner<string>>();
        RecordingSource source = new(new ProjectionReadBatch<string>(["a"], "a", hasMore: false));
        RecordingWriter writer = new(_ => new ProjectionWriteResult(writtenCount: 0, failedCount: 1));

        await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(
            "ordering",
            new ProjectionRebuildRequest("catalog-item-projections", 1),
            source,
            writer,
            CreateContext(),
            tenantScoped: true,
            CancellationToken.None));

        Assert.Empty(store.Checkpoints);
    }

    private static ServiceProvider BuildProvider(
        IReadOnlyCollection<IProjectionRebuildCheckpointStore> stores,
        RecordingReporter? reporter = null)
    {
        ServiceCollection services = new();
        services.AddMetrics();
        services.AddOptions();
        services.Configure<ApplicationIdentityOptions>(options => options.Namespace = "gma");
        services.AddProjectionRebuild();
        services.AddSingleton<ISystemClock>(new FixedClock(Now));
        services.AddSingleton<ITaskRuntimeReporter>(reporter ?? new RecordingReporter());
        services.AddSingleton<ITaskControlLoop, EmptyControlLoop>();

        foreach (IProjectionRebuildCheckpointStore store in stores)
        {
            services.AddSingleton(store);
        }

        return services.BuildServiceProvider(validateScopes: true);
    }

    private static TaskExecutionContext CreateContext() =>
        new(
            RunId,
            "ordering",
            "rebuild-catalog-item-projections",
            "projection-workers",
            "worker-a",
            "node-a",
            attempt: 1,
            tenantId: "tenant-a");

    private static MeterListener CreateListener(ICollection<MetricMeasurement> measurements)
    {
        MeterListener listener = new()
        {
            InstrumentPublished = (instrument, currentListener) =>
            {
                if (string.Equals(instrument.Meter.Name, "gma.projections", StringComparison.Ordinal))
                {
                    currentListener.EnableMeasurementEvents(instrument);
                }
            },
        };

        listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
            measurements.Add(new MetricMeasurement(instrument.Name, value, ToDictionary(tags))));
        listener.SetMeasurementEventCallback<double>((instrument, value, tags, _) =>
            measurements.Add(new MetricMeasurement(instrument.Name, value, ToDictionary(tags))));
        listener.Start();

        return listener;
    }

    private static Dictionary<string, object?> ToDictionary(
        ReadOnlySpan<KeyValuePair<string, object?>> tags) =>
        tags.ToArray().ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);

    private sealed record MetricMeasurement(
        string InstrumentName,
        double Value,
        Dictionary<string, object?> Tags);

    private sealed class RecordingSource(params ProjectionReadBatch<string>[] batches)
        : IProjectionRebuildSource<string>
    {
        private readonly Queue<ProjectionReadBatch<string>> remainingBatches = new(batches);

        public List<string?> Cursors { get; } = [];

        public Task<ProjectionReadBatch<string>> ReadAsync(
            ProjectionRebuildRequest request,
            string? cursor,
            CancellationToken cancellationToken)
        {
            this.Cursors.Add(cursor);

            return Task.FromResult(
                this.remainingBatches.Count == 0
                    ? new ProjectionReadBatch<string>([], cursor, hasMore: false)
                    : this.remainingBatches.Dequeue());
        }
    }

    private sealed class RecordingWriter(Func<int, ProjectionWriteResult> createResult)
        : IProjectionRebuildWriter<string>
    {
        public List<IReadOnlyList<string>> WrittenBatches { get; } = [];

        public Task<ProjectionWriteResult> WriteAsync(
            ProjectionRebuildRequest request,
            IReadOnlyCollection<string> snapshots,
            CancellationToken cancellationToken)
        {
            string[] batch = snapshots.ToArray();
            this.WrittenBatches.Add(batch);
            return Task.FromResult(createResult(batch.Length));
        }
    }

    private sealed class RecordingCheckpointStore(string moduleName) : IProjectionRebuildCheckpointStore
    {
        public string ModuleName { get; } = moduleName;
        public Dictionary<ProjectionRebuildCheckpointKey, ProjectionRebuildCheckpoint> Checkpoints { get; } = [];
        public int SaveCount { get; private set; }

        public Task<ProjectionRebuildCheckpoint?> GetAsync(
            ProjectionRebuildCheckpointKey key,
            CancellationToken cancellationToken)
        {
            this.Checkpoints.TryGetValue(key, out ProjectionRebuildCheckpoint? checkpoint);
            return Task.FromResult(checkpoint);
        }

        public Task SaveAsync(
            ProjectionRebuildCheckpointKey key,
            ProjectionRebuildCheckpoint checkpoint,
            CancellationToken cancellationToken)
        {
            this.Checkpoints[key] = checkpoint;
            this.SaveCount++;
            return Task.CompletedTask;
        }

        public void Seed(ProjectionRebuildCheckpointKey key, ProjectionRebuildCheckpoint checkpoint) =>
            this.Checkpoints[key] = checkpoint;
    }

    private sealed class RecordingReporter : ITaskRuntimeReporter
    {
        public List<TaskProgress> Progress { get; } = [];

        public Task ReportHeartbeatAsync(
            TaskExecutionContext context,
            DateTimeOffset observedAtUtc,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task ReportProgressAsync(
            TaskExecutionContext context,
            TaskProgress progress,
            DateTimeOffset observedAtUtc,
            CancellationToken cancellationToken)
        {
            this.Progress.Add(progress);
            return Task.CompletedTask;
        }
    }

    private sealed class EmptyControlLoop : ITaskControlLoop
    {
        public Task<TaskControlPollResult> PollAsync(
            TaskExecutionContext context,
            int maxMessages,
            CancellationToken cancellationToken) =>
            Task.FromResult(new TaskControlPollResult([]));

        public Task MarkHandledAsync(
            TaskExecutionContext context,
            TaskControlMessage message,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task MarkFailedAsync(
            TaskExecutionContext context,
            TaskControlMessage message,
            string error,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : ISystemClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
