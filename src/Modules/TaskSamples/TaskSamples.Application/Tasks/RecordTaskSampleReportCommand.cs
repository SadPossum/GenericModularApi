namespace TaskSamples.Application.Tasks;

using Shared.Application;
using Shared.Application.Cqrs;

public sealed record RecordTaskSampleReportCommand(
    string ReportName,
    int ExpectedRows,
    Guid RunId,
    string TenantId,
    int Attempt) : ICommand<Unit>;
