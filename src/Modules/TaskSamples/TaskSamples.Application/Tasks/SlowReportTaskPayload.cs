namespace TaskSamples.Application.Tasks;

using Shared.Application.Tasks;

public sealed record SlowReportTaskPayload(
    string ReportName,
    int ExpectedRows,
    int Steps,
    int DelayMilliseconds) : ITaskPayload;
