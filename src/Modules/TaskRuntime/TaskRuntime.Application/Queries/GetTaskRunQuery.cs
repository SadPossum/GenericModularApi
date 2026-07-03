namespace TaskRuntime.Application.Queries;

using Shared.Application.Cqrs;
using Shared.Application.Tasks;

public sealed record GetTaskRunQuery(Guid RunId) : IQuery<TaskRunDetails>;
