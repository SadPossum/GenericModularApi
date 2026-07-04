namespace Shared.Cqrs;

public interface ITransactionalCommand<TResponse> : ICommand<TResponse> { }
