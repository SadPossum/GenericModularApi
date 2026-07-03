namespace TaskSamples.Application.Tasks;

using Shared.Application.Tasks;

public sealed record FlakyReportTaskPayload(
    string ReportName,
    int ExpectedRows,
    int FailUntilAttempt) : ITaskPayload;
