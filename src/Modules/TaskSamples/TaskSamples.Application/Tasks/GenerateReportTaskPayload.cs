namespace TaskSamples.Application.Tasks;

using Shared.Application.Tasks;

public sealed record GenerateReportTaskPayload(string ReportName, int ExpectedRows) : ITaskPayload;
