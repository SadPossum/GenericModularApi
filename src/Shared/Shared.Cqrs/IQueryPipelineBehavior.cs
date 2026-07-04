namespace Shared.Cqrs;

using Shared.Results;

public delegate Task<Result<TResponse>> QueryNext<TResponse>();

public interface IQueryPipelineBehavior<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> HandleAsync(
        TQuery query,
        QueryNext<TResponse> next,
        CancellationToken cancellationToken);
}
