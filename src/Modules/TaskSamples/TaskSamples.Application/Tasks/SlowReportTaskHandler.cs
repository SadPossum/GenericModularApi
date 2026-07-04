namespace TaskSamples.Application.Tasks;

using Shared.Cqrs;
using Shared.Tasks;
using Shared.Tasks.Cqrs;
using Shared.Runtime.Time;
using Shared.Results;

internal sealed class SlowReportTaskHandler(
    ITaskCommandDispatcher dispatcher,
    ITaskRuntimeReporter reporter,
    ITaskControlLoop controlLoop,
    ISystemClock clock)
    : ITaskHandler<SlowReportTaskPayload>
{
    public async Task HandleAsync(
        SlowReportTaskPayload payload,
        TaskExecutionContext context,
        CancellationToken cancellationToken)
    {
        int steps = Math.Clamp(payload.Steps, 1, 20);
        int delayMilliseconds = Math.Clamp(payload.DelayMilliseconds, 0, 5_000);

        for (int step = 1; step <= steps; step++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await controlLoop
                .PauseIfRequestedAsync(context, TimeSpan.FromMilliseconds(100), maxMessages: 10, cancellationToken)
                .ConfigureAwait(false);

            if (delayMilliseconds > 0)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(delayMilliseconds), cancellationToken).ConfigureAwait(false);
            }

            await controlLoop
                .PauseIfRequestedAsync(context, TimeSpan.FromMilliseconds(100), maxMessages: 10, cancellationToken)
                .ConfigureAwait(false);

            await reporter.ReportProgressAsync(
                    context,
                    new TaskProgress(step * 100 / steps, $"step {step}/{steps}"),
                    clock.UtcNow,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        Result<Unit> result = await dispatcher.DispatchAsync<RecordTaskSampleReportCommand, Unit>(
                context,
                new RecordTaskSampleReportCommand(
                    payload.ReportName,
                    payload.ExpectedRows,
                    context.RunId,
                    context.TenantId ?? string.Empty,
                    context.Attempt),
                cancellationToken)
            .ConfigureAwait(false);

        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"Task sample report command failed with {result.Error.Code}: {result.Error.Message}");
        }
    }
}
