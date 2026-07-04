namespace TaskSamples.Application.Tasks;

using Shared.Cqrs;
using Shared.Tasks;
using Shared.Tasks.Cqrs;
using Shared.Results;

internal sealed class FlakyReportTaskHandler(ITaskCommandDispatcher dispatcher)
    : ITaskHandler<FlakyReportTaskPayload>
{
    public async Task HandleAsync(
        FlakyReportTaskPayload payload,
        TaskExecutionContext context,
        CancellationToken cancellationToken)
    {
        if (context.Attempt <= payload.FailUntilAttempt)
        {
            throw new InvalidOperationException($"Intentional sample failure on attempt {context.Attempt}.");
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
