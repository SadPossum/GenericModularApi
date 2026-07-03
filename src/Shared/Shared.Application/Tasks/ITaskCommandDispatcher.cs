namespace Shared.Application.Tasks;

using Shared.Application.Cqrs;
using Shared.ErrorHandling;

public interface ITaskCommandDispatcher
{
    Task<Result<TResponse>> DispatchAsync<TCommand, TResponse>(
        TaskExecutionContext context,
        TCommand command,
        CancellationToken cancellationToken)
        where TCommand : ICommand<TResponse>;
}
