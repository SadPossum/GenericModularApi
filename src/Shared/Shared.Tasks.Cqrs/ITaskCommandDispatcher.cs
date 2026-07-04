namespace Shared.Tasks.Cqrs;

using Shared.Cqrs;
using Shared.Results;
using Shared.Tasks;

public interface ITaskCommandDispatcher
{
    Task<Result<TResponse>> DispatchAsync<TCommand, TResponse>(
        TaskExecutionContext context,
        TCommand command,
        CancellationToken cancellationToken)
        where TCommand : ICommand<TResponse>;
}
