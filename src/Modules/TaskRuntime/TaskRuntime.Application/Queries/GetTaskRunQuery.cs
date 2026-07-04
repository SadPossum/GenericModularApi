namespace TaskRuntime.Application.Queries;

using Shared.Cqrs;
using Shared.Tasks;

public sealed record GetTaskRunQuery(Guid RunId) : IQuery<TaskRunDetails>;
