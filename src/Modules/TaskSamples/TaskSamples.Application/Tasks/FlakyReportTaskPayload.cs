namespace TaskSamples.Application.Tasks;

using Shared.Tasks;

public sealed record FlakyReportTaskPayload(
    string ReportName,
    int ExpectedRows,
    int FailUntilAttempt) : ITaskPayload;
