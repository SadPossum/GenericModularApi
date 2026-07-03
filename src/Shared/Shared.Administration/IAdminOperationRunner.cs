namespace Shared.Administration;

using Shared.ErrorHandling;

public interface IAdminOperationRunner
{
    Task<AdminOperationExecutionResult<T>> ExecuteAsync<T>(
        AdminOperationContext context,
        Func<CancellationToken, Task<Result<T>>> action,
        CancellationToken cancellationToken);
}
