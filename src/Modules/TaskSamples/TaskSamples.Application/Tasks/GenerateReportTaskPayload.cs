namespace TaskSamples.Application.Tasks;

using Shared.Tasks;

public sealed record GenerateReportTaskPayload(string ReportName, int ExpectedRows) : ITaskPayload;
