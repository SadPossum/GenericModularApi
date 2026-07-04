namespace Shared.Tasks.Cqrs;

using Shared.Cqrs;
using Shared.Results;
using Shared.Tasks;

internal sealed class TaskCommandDispatcher(IRequestDispatcher dispatcher) : ITaskCommandDispatcher
{
    public Task<Result<TResponse>> DispatchAsync<TCommand, TResponse>(
        TaskExecutionContext context,
        TCommand command,
        CancellationToken cancellationToken)
        where TCommand : ICommand<TResponse>
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(command);

        return dispatcher.SendAsync(command, cancellationToken);
    }
}
