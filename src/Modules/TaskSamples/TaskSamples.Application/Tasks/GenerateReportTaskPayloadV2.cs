namespace TaskSamples.Application.Tasks;

using Shared.Application.Tasks;

public sealed record GenerateReportTaskPayloadV2(
    string ReportName,
    int ExpectedRows,
    string Format) : ITaskPayload;
